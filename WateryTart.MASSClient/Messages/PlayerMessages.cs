using System.Collections;
using WateryTart.MassClient.Models;

namespace WateryTart.MassClient.Messages;

public class PlayerMessages : MessageFactoryBase
{
    public static MessageBase PlayersAll => JustCommand(Commands.PlayersAll);

    public static MessageBase PlayerQueuesAll => JustCommand(Commands.PlayerQueuesAll);
    public static MessageBase PlayerQueueItems(string queueid) => JustId(Commands.PlayerQueueItems, queueid, "queue_id"); 

    public static MessageBase PlayerNext(string playerId) => JustId(Commands.PlayerNext, playerId, "player_id");
    public static MessageBase PlayerPlay(string playerId) => JustId(Commands.PlayerPlay, playerId, "player_id");
    public static MessageBase PlayerPlayPause(string playerId) => JustId(Commands.PlayerPlayPause, playerId, "player_id");
    public static MessageBase PlayerPrevious(string playerId) => JustId(Commands.PlayerPrevious, playerId, "player_id");
    public static MessageBase PlayerActiveQueue(string playerId) => JustId(Commands.PlayerActiveQueue, playerId, "player_id");

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