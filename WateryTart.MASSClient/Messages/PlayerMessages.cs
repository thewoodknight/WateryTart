using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Security.Cryptography;
using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Messages;

public class PlayerMessages : MessageFactoryBase
{
    public static MessageBase PlayersAll => JustCommand(Commands.PlayersAll);

    public static MessageBase PlayerQueuesAll => JustCommand(Commands.PlayerQueuesAll);

    public static MessageBase PlayerActiveQueue(string playerId)
    {
        var m = new Message(Commands.PlayerActiveQueue)
        {
            args = new Hashtable
            {
                { "player_id", playerId },
            }
        };

        return m;
    }

    public static MessageBase PlayerQueuePlayMedia(string queue_id, MediaItemBase media, PlayMode mode = PlayMode.Play)
    {
        
        /* Unsure why this didn't serialise the enum, perhaps because it was in a hashtable */
        var modestr = "";
        switch (mode)
        {
            case PlayMode.Play:
                modestr = "play";
                break;
            case PlayMode.Replace:
                modestr = "replace";
                break;
            case PlayMode.Next:
                modestr = "next";
                break;
            case PlayMode.ReplaceNext:
                modestr = "replace_next";
                break;
            case PlayMode.Add:
                modestr = "add";
                break;
            case PlayMode.Unknown:
                modestr = "unknown";
                break;
        }

        var m = new Message(Commands.PlayerQueuePlayMedia)
        {
            args = new Hashtable
            {
                { "queue_id", queue_id },
                { "media", media},
                { "option", mode}
            }
        };

        return m;
    }
}