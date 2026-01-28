using Newtonsoft.Json;
using System.Collections;

namespace WateryTart.MassClient.Messages;

public abstract class MessageFactoryBase
{
    public static MessageBase JustCommand(string command)
    {
        return new Message(command);
    }
    public static MessageBase JustId(string command, string id, string id_label = "item_id")
    {
        var m = new Message(command)
        {
            args = new Hashtable
            {
                { id_label, id },
            }
        };

        return m;
    }
    public static MessageBase IdAndProvider(string command, string id, string provider)
    {
        var m = new Message(command)
        {
            args = new Hashtable
            {
                { "item_id", id },
                { "provider_instance_id_or_domain", provider }
            }
        };

        return m;
    }

    public static string ToJson(MessageBase message)
    {
        return JsonConvert.SerializeObject(message);
    }
}