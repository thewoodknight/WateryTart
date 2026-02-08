using System.Collections;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    extension(IMassWsClient c)
    {
        public void GetAuthToken(string username, string password, Action<AuthResponse> responseHandler)
        {
            var m = new Message(Commands.AuthLogin)
            {
                args = new Dictionary<string, object>()
                {
                    { "username", username },
                    { "password", password }
                }
            };

            c.Send<AuthResponse>(m, Deserialise<AuthResponse>(responseHandler), true);
        }
    }
}