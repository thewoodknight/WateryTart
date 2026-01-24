using System.Collections;

namespace WateryTart.MassClient.Messages;

public static class AuthMessages
{
    public static MessageBase Login(string username, string password)
    {
        var m = new Message(Commands.AuthLogin)
        {
            args = new Hashtable
            {
                { "username", username },
                { "password", password }
            }
        };

        return m;
    }
}