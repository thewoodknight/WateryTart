using WateryTart.Service.MassClient.Events;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Models.Auth;

namespace WateryTart.Service.MassClient;

public interface IMassWsClient
{
    Task<MassCredentials> Login(string username, string password, string baseurl);

    Task<bool> Connect(IMassCredentials credentials);

    void Send<T>(MessageBase message, Action<string> ResponseHandler, bool ignoreConnection = false);
    Task DisconnectAsync();

    bool IsConnected { get; }

    IObservable<BaseEventResponse> Events { get; }
}