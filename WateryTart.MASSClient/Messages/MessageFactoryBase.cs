using Newtonsoft.Json;

namespace WateryTart.MassClient.Messages;

public abstract class MessageFactoryBase
{
    public static MessageBase JustCommand(string command)
    {
        return new Message(command);
    }
    public static string ToJson(MessageBase message)
    {
        return JsonConvert.SerializeObject(message);
    }
}