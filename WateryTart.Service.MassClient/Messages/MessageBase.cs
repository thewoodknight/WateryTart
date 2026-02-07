using System.Text.Json;

namespace WateryTart.Service.MassClient.Messages;

public abstract class MessageBase(string command)
{
    public Dictionary<string, object>? args { get; set; }
    public string message_id { get; set; } = Guid.NewGuid().ToString();
    public string command { get; set; } = command;

    public string ToJson()
    {
        return this switch
        {
            Message msg => JsonSerializer.Serialize(msg, MassClientJsonContext.Default.Message),
            Auth auth => JsonSerializer.Serialize(auth, MassClientJsonContext.Default.Auth),
            _ => JsonSerializer.Serialize(this, MassClientJsonContext.Default.GetTypeInfo(this.GetType())!)
        };
    }
}
