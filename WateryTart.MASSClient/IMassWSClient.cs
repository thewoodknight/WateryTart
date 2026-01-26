using WateryTart.MassClient.Events;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models.Auth;

namespace WateryTart.MassClient;

public interface IMassWsClient
{
    Task<MassCredentials> Login(string username, string password, string baseurl);

    Task Connect(IMassCredentials credentials);

    void Send<T>(MessageBase message, Action<string> ResponseHandler, bool ignoreConnection = false);

    bool IsConnected { get; }

    IObservable<BaseEventResponse> Events { get;  }
}