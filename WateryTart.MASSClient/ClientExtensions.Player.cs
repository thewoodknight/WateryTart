using System.Collections;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;

namespace WateryTart.MassClient;


public static partial class MassClientExtensions
{
    extension(IMassWsClient c)
    {
        public void Play(string queueID, MediaItemBase t, PlayMode mode, Action<PlayersQueuesResponse> responseHandler)
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
                    { "queue_id", queueID },
                    { "media", t},
                    { "option", mode}
                }
            };


            c.Send<PlayerResponse>(m, Deserialise<PlayersQueuesResponse>(responseHandler));
        }

        public void PlayerNext(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustId(Commands.PlayerNext, playerId, "player_id"), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPlay(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustId(Commands.PlayerPlay, playerId, "player_id"), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPlayPause(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustId(Commands.PlayerPlayPause, playerId, "player_id"), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPrevious(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustId(Commands.PlayerPrevious, playerId, "player_id"), Deserialise<PlayersQueuesResponse>(responseHandler));
        }

        public void PlayersAll(Action<PlayerResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustCommand(Commands.PlayersAll), Deserialise<PlayerResponse>(responseHandler));
        }

        public void PlayerQueuesAll(Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustCommand(Commands.PlayerQueuesAll), Deserialise<PlayersQueuesResponse>(responseHandler));
        }

        public void PlayerActiveQueue(string id, Action<PlayerQueueResponse> responseHandler)
        {
            c.Send<PlayerQueueResponse>(JustId(Commands.PlayerActiveQueue, id, "player_id"), Deserialise<PlayerQueueResponse>(responseHandler));
        }
        public void PlayerQueueItems(string id, Action<PlayerQueueItemsResponse> responseHandler)
        {
            c.Send<PlayerQueueItemsResponse>(JustId(Commands.PlayerQueueItems, id, "queue_id"), Deserialise<PlayerQueueItemsResponse>(responseHandler));
        }

        public void PlayerVolumeUp(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(JustId(Commands.PlayerGroupVolumeUp, playerId, "player_id"), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
    }
}