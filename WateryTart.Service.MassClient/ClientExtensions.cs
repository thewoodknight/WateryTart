using Newtonsoft.Json;
using System.Collections;
using WateryTart.Service.MassClient.Messages;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    internal static Task<T> SendAsync<T>(IMassWsClient client, MessageBase message)
    {
        var tcs = new TaskCompletionSource<T>();
        try
        {
            client.Send<T>(message, (response) =>
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(response);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
        return tcs.Task;
    }

    private static MessageBase JustCommand(string command)
    {
        return new Message(command);
    }

    private static MessageBase JustId(string command, string id, string id_label = "item_id")
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

    private static MessageBase IdAndProvider(string command, string id, string provider)
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

    private static Action<string> Deserialise<T>(Action<T> responseHandler)
    {
        Action<string> d = (r) =>
        {
            if (r == null)
                return;

            var y = JsonConvert.DeserializeObject<T>(r);
            responseHandler(y);
        };

        return d;
    }
}