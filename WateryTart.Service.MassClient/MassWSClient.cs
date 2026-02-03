using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using WateryTart.Service.MassClient.Events;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Auth;
using WateryTart.Service.MassClient.Responses;
using Websocket.Client;

namespace WateryTart.Service.MassClient
{
    public class MassWsClient : IMassWsClient
    {
        private string _baseUrl;
        internal WebsocketClient _client;
        internal Dictionary<string, Action<string>> _routing = new();
        private IMassCredentials creds;

        private CancellationTokenSource _connectionCts = new CancellationTokenSource();
        private readonly Subject<BaseEventResponse> subject = new Subject<BaseEventResponse>();
        private IDisposable _reconnectionSubscription;
        private IDisposable _messageSubscription;

        // Track if we're already attempting to connect
        private Task _currentConnectTask = Task.CompletedTask;
        private readonly object _connectLock = new object();

        public MassWsClient()
        {
            JsonSerializerSettings s = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new LowercaseNamingPolicy(),

                }
            };
            s.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            JsonConvert.DefaultSettings = () => s;
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
                Console.WriteLine($"Connecting to WebSocket: {wsUrl}");
                _client.MessageReceived.Subscribe(OnNext);
                await _client.Start();
                Console.WriteLine("WebSocket connected, sending auth request");

                this.GetAuthToken(username, password, (response) =>
                {
                    Console.WriteLine($"Auth response received: success={response?.Result?.success}");
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
            // Only allow one Connect attempt at a time
            lock (_connectLock)
            {
                if (!_currentConnectTask.IsCompleted)
                {
                    Console.WriteLine("Connect already in progress, returning existing task");
                    return false;
                }
            }

            creds = credentials;

            _reconnectionSubscription?.Dispose();
            _messageSubscription?.Dispose();
            _client?.Dispose();
            _connectionCts = new CancellationTokenSource();

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(1) }
            });

            // Music Assistant uses port 8095 for HTTP and 8097 for WebSocket API
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

            try
            {
                await Task.Delay(Timeout.Infinite, _connectionCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Connect cancelled");
            }

            return !_connectionCts.Token.IsCancellationRequested;
        }

        /// <summary>
        /// Converts a base URL (e.g., "192.168.1.63:8095") to the WebSocket URL.
        /// Music Assistant WebSocket is on the same port as HTTP.
        /// </summary>
        private static string GetWebSocketUrl(string baseUrl)
        {
            // WebSocket is on the same port as HTTP, just different protocol
            return $"ws://{baseUrl}/ws";
        }

        private void SendLogin(IMassCredentials credentials)
        {
            var argsx = new Hashtable { { "token", credentials.Token } };
            var auth = new Auth()
            {
                message_id = "auth-" + Guid.NewGuid(),
                args = argsx
            };
            _client?.Send(JsonConvert.SerializeObject(auth));
        }

        public void Send<T>(MessageBase message, Action<string> responseHandler, bool ignoreConnection = false)
        {
            var json = message.ToJson();
            Console.WriteLine($"WS Sending: {json}");
            _routing.Add(message.message_id, responseHandler);

            if (!ignoreConnection && (_client == null || !_client.IsRunning))
            {
                // Only start a new Connect task if one isn't already running
                lock (_connectLock)
                {
                    if (_currentConnectTask.IsCompleted)
                    {
                        Console.WriteLine("Starting new Connect task from Send");
                        _currentConnectTask = ConnectSafely();
                    }
                }
            }

            _client?.Send(message.ToJson());
        }

        private async Task ConnectSafely()
        {
            try
            {
                await Connect(creds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connect error: {ex}");
            }
        }

        public bool IsConnected => (_client != null && _client.IsRunning);

        private void OnNext(ResponseMessage response)
        {
            if (string.IsNullOrEmpty(response.Text))
                return;

            Console.WriteLine($"WS Received: {response.Text.Substring(0, Math.Min(200, response.Text.Length))}...");

            // Skip server info messages but NOT auth responses
            if (response.Text.Contains("\"server_id\"") && !response.Text.Contains("\"message_id\""))
            {
                Console.WriteLine("Skipping server info message");
                return;
            }

            TempResponse y = JsonConvert.DeserializeObject<TempResponse>(response.Text);
            Console.WriteLine($"Parsed message_id: {y?.message_id}, routing keys: {string.Join(",", _routing.Keys)}");
            if (y?.message_id != null && _routing.ContainsKey(y.message_id))
            {
                Console.WriteLine($"Found routing match for {y.message_id}");
                _routing[y.message_id].Invoke(response.Text);
                _routing.Remove(y.message_id);
                return;
            }

            var e = JsonConvert.DeserializeObject<BaseEventResponse>(response.Text);

            switch (e.EventName)
            {
                case EventType.MediaItemPlayed:
                    subject.OnNext(JsonConvert.DeserializeObject<MediaItemEventResponse>(response.Text));
                    break;
                case EventType.PlayerAdded:
                case EventType.PlayerUpdated:
                case EventType.PlayerRemoved:
                case EventType.PlayerConfigUpdated:
                    try
                    {
                        var x = JsonConvert.DeserializeObject<PlayerEventResponse>(response.Text);
                        subject.OnNext(x);
                    }
                    catch (Exception ex)
                    {

                    }

                    break;

                case EventType.QueueAdded:
                case EventType.QueueUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerQueueEventResponse>(response.Text));
                    break;
                case EventType.QueueItemsUpdated:
                    break;
                case EventType.QueueTimeUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerQueueTimeUpdatedEventResponse>(response.Text));
                    break;

                default:
                    subject.OnNext(e);
                    break;
            }
        }

        public IObservable<BaseEventResponse> Events => subject;

        public async Task DisconnectAsync()
        {
            Console.WriteLine("MassWsClient Disconnecting...");

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
                        Console.WriteLine("Stopping WebSocket client...");
                        try
                        {
                            await _client.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error calling Stop: {ex}");
                        }

                        // Force abort if still running
                        if (_client.IsRunning)
                        {
                            Console.WriteLine("WebSocket still running, attempting abort...");
                            _client.NativeClient?.Abort();
                        }

                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping WebSocket: {ex}");
            }

            try
            {
                _reconnectionSubscription?.Dispose();
                _messageSubscription?.Dispose();
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing WebSocket: {ex}");
            }

            try
            {
                _connectionCts?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing CTS: {ex}");
            }

            try
            {
                subject?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing subject: {ex}");
            }

            Console.WriteLine("MassWsClient Disconnected");
        }
    }
}