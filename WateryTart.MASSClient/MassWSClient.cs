using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using WateryTart.MassClient.Events;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Models.Auth;
using WateryTart.MassClient.Responses;
using Websocket.Client;

namespace WateryTart.MassClient
{
    public class MassWsClient : IMassWsClient
    {
        private string _baseUrl;
        internal WebsocketClient _client;

        internal Dictionary<string, Action<string>> _routing = new(); // Can this be converted to something with types?
        private IMassCredentials creds;

        public MassWsClient()
        {
            JsonSerializerSettings s = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new LowercaseNamingPolicy() //this is ignored by enums
                }
            };
            s.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            JsonConvert.DefaultSettings = () => s;

        }
        public async Task<MassCredentials> Login(string username, string password, string baseurl)
        {
            var result = await Task.Run(() =>
             {
                 MassCredentials mc = new MassCredentials();

                 var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                 {
                     Options =
                     {
                        KeepAliveInterval = TimeSpan.FromSeconds(1),
                     }
                 });

                 var exitEvent = new ManualResetEvent(false);
                 using (_client = new WebsocketClient(new Uri($"ws://{baseurl}/ws"), factory))
                 {
                     _client
                         .MessageReceived
                         .Subscribe(OnNext);

                     _client.Start();

                     this.GetAuthToken(username, password, (response) =>
                    {
                        var x = response;

                        if (!response.Result.success)
                            return;

                        mc = new MassCredentials()
                        {
                            Token = response.Result.access_token,
                            BaseUrl = baseurl
                        };

                        exitEvent.Set();
                    });

                     exitEvent.WaitOne();
                 }
                 return mc;
             });

            return result;
        }

        public async Task Connect(IMassCredentials credentials)
        {
            creds = credentials;
            _ = Task.Run(() =>
            {
                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(1),
                    }
                });

                var exitEvent = new ManualResetEvent(false);
                using (_client = new WebsocketClient(new Uri($"ws://{credentials.BaseUrl}/ws"), factory))
                {
                    _client.ReconnectionHappened.Subscribe(info =>
                        Debug.WriteLine($"Reconnection happened, type: {info.Type}"));

                    _client.MessageReceived.Subscribe(OnNext);
                    _client.Start();

                    Login(credentials);

                    exitEvent.WaitOne();
                }
            });
        }

        private void Login(IMassCredentials credentials)
        {
            var argsx = new Hashtable
            {
                { "token", credentials.Token }
            };
            var auth = new Auth()
            {
                message_id = "auth-123",
                args = argsx
            };
            _client.Send(JsonConvert.SerializeObject(auth));
        }

        public void Send<T>(MessageBase message, Action<string> responseHandler, bool ignoreConnection = false)
        {
            _routing.Add(message.message_id, responseHandler);

            if (!ignoreConnection)

                if (!_client.IsRunning)
                {
                    Connect(creds);
                }
            _client.Send(message.ToJson());
        }
        
        public bool IsConnected => (_client != null && _client.IsRunning);

        private void OnNext(ResponseMessage response)
        {
            Console.WriteLine(response.Text);

            if (string.IsNullOrEmpty(response.Text))
                return;

            if (response.Text.Contains("\"server_id\""))
                //this would be the initial startup server details
                return;

            if (response.Text.Contains("\"auth-123\""))
                //this would be the initial startup authorisation details
                return;

            //Responses from messages
            TempResponse y = JsonConvert.DeserializeObject<TempResponse>(response.Text);
            if (y?.message_id != null && _routing.ContainsKey(y.message_id))
            {
                _routing[y.message_id].Invoke(response.Text);
                _routing.Remove(y.message_id);
                return;
            }


            //Event handling
            var e = JsonConvert.DeserializeObject<BaseEventResponse>(response.Text);

            switch (e.EventName)
            {
                case EventType.PlayerAdded:
                case EventType.PlayerUpdated:
                case EventType.PlayerRemoved:
                case EventType.PlayerConfigUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerEventResponse>(response.Text));
                    Debug.WriteLine(response.Text);
                    break;

                case EventType.QueueAdded:
                case EventType.QueueUpdated:
                case EventType.QueueItemsUpdated:
                case EventType.QueueTimeUpdated:
                    subject.OnNext(JsonConvert.DeserializeObject<PlayerQueueEventResponse>(response.Text));
                    Debug.WriteLine(response.Text);
                    break;
                default:
                    subject.OnNext(e);
                    break;
            }
        }
        private readonly Subject<BaseEventResponse> subject = new Subject<BaseEventResponse>();

        public IObservable<BaseEventResponse> Events
        {
            get { return subject; }
        }
    }
}