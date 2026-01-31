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
                    NamingStrategy = new LowercaseNamingPolicy()
                }
            };
            s.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            JsonConvert.DefaultSettings = () => s;
        }

        public async Task<MassCredentials> Login(string username, string password, string baseurl)
        {
            MassCredentials mc = new MassCredentials();

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options = { KeepAliveInterval = TimeSpan.FromSeconds(1) }
            });

            var tcs = new TaskCompletionSource<MassCredentials>();
            using (_client = new WebsocketClient(new Uri($"ws://{baseurl}/ws"), factory))
            {
                _client.MessageReceived.Subscribe(OnNext);
                _client.Start();

                this.GetAuthToken(username, password, (response) =>
                {
                    if (!response.Result.success)
                    {
                        tcs.TrySetResult(new MassCredentials());
                        return;
                    }

                    mc = new MassCredentials()
                    {
                        Token = response.Result.access_token,
                        BaseUrl = baseurl
                    };

                    tcs.TrySetResult(mc);
                });

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
                    Debug.WriteLine("Connect already in progress, returning existing task");
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

            _client = new WebsocketClient(new Uri($"ws://{credentials.BaseUrl}/ws"), factory);

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
                Debug.WriteLine("Connect cancelled");
            }

            return !_connectionCts.Token.IsCancellationRequested;
        }

        private void SendLogin(IMassCredentials credentials)
        {
            var argsx = new Hashtable { { "token", credentials.Token } };
            var auth = new Auth()
            {
                message_id = "auth-123",
                args = argsx
            };
            _client?.Send(JsonConvert.SerializeObject(auth));
        }

        public void Send<T>(MessageBase message, Action<string> responseHandler, bool ignoreConnection = false)
        {
            _routing.Add(message.message_id, responseHandler);

            if (!ignoreConnection && (_client == null || !_client.IsRunning))
            {
                // Only start a new Connect task if one isn't already running
                lock (_connectLock)
                {
                    if (_currentConnectTask.IsCompleted)
                    {
                        Debug.WriteLine("Starting new Connect task from Send");
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
                Debug.WriteLine($"Connect error: {ex}");
            }
        }

        public bool IsConnected => (_client != null && _client.IsRunning);

        private void OnNext(ResponseMessage response)
        {
            if (string.IsNullOrEmpty(response.Text))
                return;

            if (response.Text.Contains("\"server_id\"") || response.Text.Contains("\"auth-123\""))
                return;

            TempResponse y = JsonConvert.DeserializeObject<TempResponse>(response.Text);
            if (y?.message_id != null && _routing.ContainsKey(y.message_id))
            {
                _routing[y.message_id].Invoke(response.Text);
                _routing.Remove(y.message_id);
                return;
            }

            var e = JsonConvert.DeserializeObject<BaseEventResponse>(response.Text);

            switch (e.EventName)
            {
                case EventType.PlayerAdded:
                case EventType.PlayerUpdated:
                case EventType.PlayerRemoved:
                case EventType.PlayerConfigUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerEventResponse>(response.Text));

                    break;

                case EventType.QueueAdded:
                case EventType.QueueUpdated:
                case EventType.QueueItemsUpdated:
                case EventType.QueueTimeUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerQueueEventResponse>(response.Text));

                    break;

                default:
                    subject.OnNext(e);
                    break;
            }
        }

        public IObservable<BaseEventResponse> Events => subject;

        public async Task DisconnectAsync()
        {
            Debug.WriteLine("MassWsClient Disconnecting...");

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
                        Debug.WriteLine("Stopping WebSocket client...");
                        try
                        {
                            await _client.Stop(WebSocketCloseStatus.NormalClosure, "Shutdown");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error calling Stop: {ex}");
                        }

                        // Force abort if still running
                        if (_client.IsRunning)
                        {
                            Debug.WriteLine("WebSocket still running, attempting abort...");
                            _client.NativeClient?.Abort();
                        }

                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping WebSocket: {ex}");
            }

            try
            {
                _reconnectionSubscription?.Dispose();
                _messageSubscription?.Dispose();
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing WebSocket: {ex}");
            }

            try
            {
                _connectionCts?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing CTS: {ex}");
            }

            try
            {
                subject?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing subject: {ex}");
            }

            Debug.WriteLine("MassWsClient Disconnected");
        }
    }
}