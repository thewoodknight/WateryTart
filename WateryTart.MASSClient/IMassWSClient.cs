using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models.Auth;
using WateryTart.MassClient.Responses;

namespace WateryTart.MassClient;

public interface IMassWSClient
{
    public Task<MassCredentials> Login(string username, string password, string baseurl);
    public Task Connect(IMassCredentials credentialss);
    public void Send<T>(MessageBase message, Action<string> ResponseHandler);

    public void Send<T>(MessageBase message, Action<ResponseBase> ResponseHandler);
    bool IsConnected { get; }
}