using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using WateryTart.MassClient.Events;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models.Auth;
using WateryTart.MassClient.Responses;
using Websocket.Client;

namespace WateryTart.MassClient
{
    public class MassWsClient : IMassWSClient
    {
        private string _baseUrl;
        internal WebsocketClient _client;

        internal Dictionary<string, Action<string>> _routing = new(); // Can this be converted to something with types?
        internal Dictionary<string, Action<ResponseBase>> _routing2 = new();

        public MassWsClient()
        {

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
                 using (_client = new WebsocketClient(new Uri(baseurl), factory))
                 {
                     _client.MessageReceived.Subscribe(OnNext);
                     _client.Start();


                     this.GetAuthToken(username, password, (response) =>
                    {
                        var x = response;

                        if (!response.result.success)
                            return;

                        mc = new MassCredentials()
                        {
                            Token = response.result.access_token,
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
                using (_client = new WebsocketClient(new Uri(credentials.BaseUrl), factory))
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

        public void Send<T>(MessageBase message, Action<string> responseHandler)
        {
            _routing.Add(message.message_id, responseHandler);
            _client.Send(message.ToJson());
        }

        public void Send<T>(MessageBase message, Action<ResponseBase> responseHandler)
        {
            _routing2.Add(message.message_id, responseHandler);
            _client.Send(message.ToJson());
        }

        public bool IsConnected => (_client != null && _client.IsRunning);

        private void OnNext(ResponseMessage response)
        {
            if (string.IsNullOrEmpty(response.Text))
                return;

            //Responses from messages
            TempResponse y = JsonConvert.DeserializeObject<TempResponse>(response.Text);
            if (y.message_id != null && _routing.ContainsKey(y.message_id))
            {
                _routing[y.message_id].Invoke(response.Text);
                return;
            }

            //Event handling
            var e = JsonConvert.DeserializeObject<EventResponse>(response.Text);
            Debug.WriteLine(e.EventName);
        }
    }
}
