using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Events;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Models.Auth;
using WateryTart.Service.MassClient.Models.Enums;
using WateryTart.Service.MassClient.Responses;
using Websocket.Client;

namespace WateryTart.Service.MassClient
{
    public class MassWsClient : IMassWsClient
    {
        internal WebsocketClient? _client;
        internal ConcurrentDictionary<string, Action<string>> _routing = new();
        private IMassCredentials? creds;

        private CancellationTokenSource _connectionCts = new CancellationTokenSource();
        private readonly Subject<BaseEventResponse?> subject = new Subject<BaseEventResponse?>();
        private IDisposable? _reconnectionSubscription;
        private IDisposable? _messageSubscription;

        // Track if we're already attempting to connect
        private Task _currentConnectTask = Task.CompletedTask;
        private readonly object _connectLock = new object();

        private bool _isAuthenticated = false;
        private readonly Queue<string> _pendingMessages = new();
        private readonly object _authLock = new object();

        private readonly ILogger logger;
        /// <summary>
        /// Shared JSON serializer options for all MassClient operations. AOT-compatible with snake_case naming.
        /// </summary>
        internal static readonly JsonSerializerOptions SerializerOptions = new()
        {
            TypeInfoResolver = MassClientJsonContext.Default,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false

        };

        public MassWsClient()
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            logger = factory.CreateLogger("MassWsClient");
        }

        public async Task<LoginResults> Login(string username, string password, string baseurl)
        {
            MassCredentials mc = new MassCredentials();

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(1) }
            });

            // Music Assistant uses port 8095 for HTTP and 8097 for WebSocket API
            var wsUrl = GetWebSocketUrl(baseurl);

            var tcs = new TaskCompletionSource<LoginResults>();
            using (_client = new WebsocketClient(new Uri(wsUrl), factory))
            {
                logger.LogInformation("Connecting to WebSocket: {WsUrl}", wsUrl);
                _client.MessageReceived.Subscribe(OnNext);
                await _client.Start();
                logger.LogInformation("WebSocket connected, sending auth request");

                this.GetAuthToken(username, password, (response) =>
                {
                    logger.LogInformation("Auth response received: success={Success}", response?.Result?.success);
                    if (response?.Result == null)
                    {
                        tcs.TrySetResult(new LoginResults { Success = false, Error = "No response from server" });
                        return;
                    }
                    if (!response.Result.success)
                    {
                        var r = new LoginResults
                        {
                            Success = false,
                            Error = response.Result.error ?? "Authentication failed"

                        };
                        tcs.TrySetResult(r);
                        return;
                    }
                    var success = new LoginResults
                    {
                        Credentials = new MassCredentials
                        {
                            Token = response.Result.access_token,
                            BaseUrl = baseurl
                        },
                        Success = true
                    };
                    tcs.TrySetResult(success);
                });

                // Add timeout to prevent hanging forever
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    return new LoginResults { Success = false, Error = "Connection timed out" };
                }

                return await tcs.Task;
            }
        }

        public async Task<bool> Connect(IMassCredentials credentials)
        {
            var x = new MassClientJsonContext();
            logger.LogInformation("WS Connecting");

            lock (_connectLock)
            {
                if (!_currentConnectTask.IsCompleted)
                {
                    return false;
                }
            }

            creds = credentials;
            _isAuthenticated = false;

            _reconnectionSubscription?.Dispose();
            _messageSubscription?.Dispose();
            _client?.Dispose();
            _connectionCts = new CancellationTokenSource();

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(1) }
            });

            if (credentials.BaseUrl == null)
            {
                logger.LogError("No credential base url found");
                return false;
            }

            var wsUrl = GetWebSocketUrl(credentials.BaseUrl);
            _client = new WebsocketClient(new Uri(wsUrl), factory);

            _reconnectionSubscription = _client.ReconnectionHappened.Subscribe(info =>
            {
                if (!_connectionCts.Token.IsCancellationRequested)
                {
                    SendLogin(credentials);
                }
            });

            _messageSubscription = _client.MessageReceived.Subscribe(OnNext);

            await _client.Start();
            SendLogin(credentials);

            var authTimeout = Task.Delay(TimeSpan.FromSeconds(10), _connectionCts.Token);
            var authCompleted = WaitForAuthenticationAsync();

            var completedTask = await Task.WhenAny(authCompleted, authTimeout);

            if (completedTask == authTimeout)
            {
                logger.LogWarning("Authentication timeout");
                return false;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, _connectionCts.Token);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Background connection task cancelled");
                }
            });

            return _isAuthenticated;
        }

        /// <summary>
        /// Converts a base URL (e.g., "192.168.1.63:8095") to the WebSocket URL.
        /// Music Assistant WebSocket is on the same port as HTTP, just different protocol.
        /// </summary>
        private static string GetWebSocketUrl(string baseUrl)
        {
            // WebSocket is on the same port as HTTP, just different protocol
            return $"ws://{baseUrl}/ws";
        }

        private void SendLogin(IMassCredentials credentials)
        {
            logger.LogInformation("Sending authentication...");
            var argsx = new Dictionary<string, object>() { { "token", credentials.Token } };
            var auth = new Auth()
            {
                message_id = "auth-" + Guid.NewGuid(),
                args = argsx
            };

            _routing.TryAdd(auth.message_id, (response) =>
            {
                logger.LogInformation("Auth response: {Response}", response);

                if (!response.Contains("error"))
                {
                    lock (_authLock)
                    {
                        _isAuthenticated = true;

                        // Send all pending messages
                        while (_pendingMessages.Count > 0)
                        {
                            var pending = _pendingMessages.Dequeue();
                            _client?.Send(pending);
                        }
                    }
                }
            });

            // Use JsonSerializer.Serialize with JsonTypeInfo to avoid RequiresUnreferencedCode warning
            var json = JsonSerializer.Serialize(auth, MassClientJsonContext.Default.Auth);
            logger.LogInformation("Sending auth: {Json}", json);
            _client?.Send(json);
        }

        public void Send<T>(MessageBase message, Action<string> responseHandler, bool ignoreConnection = false)
        {

            var json = message.ToJson();
            _routing.TryAdd(message.message_id, responseHandler);  // Changed from Add

            if (!ignoreConnection && (_client == null || !_client.IsRunning))
            {
                lock (_connectLock)
                {
                    if (_currentConnectTask.IsCompleted)
                    {
                        _currentConnectTask = ConnectSafely();
                    }
                }
            }

            _client?.Send(json);
        }

        private async Task ConnectSafely()
        {
            logger.LogDebug("WS Connecting");
            try
            {
                if (creds != null) 
                    await Connect(creds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connect error");
            }
        }

        public bool IsConnected => (_client != null && _client.IsRunning);

        private void OnNext(ResponseMessage response)
        {
            if (string.IsNullOrEmpty(response.Text))
                return;

            if (response.Text.Contains("\"server_id\"") && !response.Text.Contains("\"message_id\""))
            {
                return;
            }

            // Use JsonSerializer.Deserialize with JsonTypeInfo to avoid RequiresUnreferencedCode warning
            TempResponse? y = JsonSerializer.Deserialize(response.Text, MassClientJsonContext.Default.TempResponse);

            // Use TryRemove instead of ContainsKey + indexer
            if (y?.message_id != null && _routing.TryRemove(y.message_id, out var handler))
            {
                handler?.Invoke(response.Text);
                return;
            }

            try
            {
                var e = JsonSerializer.Deserialize<BaseEventResponse>(response.Text, SerializerOptions);
                if (e == null)
                    return;

                switch (e.EventName)
                {
                    case EventType.MediaItemPlayed:
                        subject.OnNext(JsonSerializer.Deserialize(response.Text, MassClientJsonContext.Default.MediaItemEventResponse));
                        break;
                    case EventType.PlayerAdded:
                    case EventType.PlayerUpdated:
                    case EventType.PlayerRemoved:
                    case EventType.PlayerConfigUpdated:
                        try
                        {
                            var x = JsonSerializer.Deserialize(response.Text, MassClientJsonContext.Default.PlayerEventResponse);
                            subject.OnNext(x);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error in player config update");
                        }

                        break;

                    case EventType.QueueAdded:
                    case EventType.QueueUpdated:
                        subject.OnNext(JsonSerializer.Deserialize(response.Text, MassClientJsonContext.Default.PlayerQueueEventResponse));
                        break;
                    case EventType.QueueItemsUpdated:
                        break;
                    case EventType.QueueTimeUpdated:
                        subject.OnNext(JsonSerializer.Deserialize(response.Text, MassClientJsonContext.Default.PlayerQueueTimeUpdatedEventResponse));
                        break;

                    default:
                        subject.OnNext(e);
                        break;
                }
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON Deserialization Error");
                logger.LogDebug("Path: {Path}", ex.Path);
            }
        }

        public IObservable<BaseEventResponse> Events => subject;

        public async Task DisconnectAsync()
        {
            try
            {
                // Cancel the connection immediately
                _connectionCts?.Cancel();
            }
            catch { }

            try
            {
                if (_client != null)
                {
                    // Disable reconnection first
                    _client.IsReconnectionEnabled = false;

                    if (_client.IsRunning)
                    {
                        try
                        {
                            await _client.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error calling Stop");
                        }

                        // Force abort if still running
                        if (_client.IsRunning)
                        {
                            logger.LogDebug("WebSocket still running, attempting abort...");
                            _client.NativeClient?.Abort();
                        }

                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error stopping WebSocket");
            }

            try
            {
                _reconnectionSubscription?.Dispose();
                _messageSubscription?.Dispose();
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error disposing WebSocket");
            }

            try
            {
                _connectionCts?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error disposing CTS");
            }

            try
            {
                subject?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error disposing subject");
            }
        }

        private async Task WaitForAuthenticationAsync()
        {
            while (!_isAuthenticated && !_connectionCts.Token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }
    }
}