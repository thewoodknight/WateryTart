namespace WateryTart.MassClient.Messages;

public class Auth : MessageBase
{
    public Auth() : base(Commands.Auth)
    {
    }
}