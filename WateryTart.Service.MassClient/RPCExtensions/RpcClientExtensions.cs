using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    extension(MassRpcClient c)
    {
        public async Task<List<Player>?> PlayersAllAsync()
        {
            return await c.Send<List<Player>>(JustCommand(Commands.PlayersAll));
        }

        public async Task<List<PlayerQueue>?> PlayerNextAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerNext, playerId, "player_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerPlayAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerPlay, playerId, "player_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerPlayPauseAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerPlayPause, playerId, "player_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerPreviousAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerPrevious, playerId, "player_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerQueuesAllAsync()
        {
            return await c.Send<List<PlayerQueue>>(JustCommand(Commands.PlayerQueuesAll));
        }

        public async Task<PlayerQueue?> PlayerActiveQueueAsync(string id)
        {
            return await c.Send<PlayerQueue>(JustId(Commands.PlayerActiveQueue, id, "player_id"));
        }
        
        public async Task<List<PlayerQueue>?> PlayerQueueItemsAsync(string id)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerQueueItems, id, "queue_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerGroupVolumeUpAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerGroupVolumeUp, playerId, "player_id"));
        }

        public async Task<List<PlayerQueue>?> PlayerGroupVolumeDownAsync(string playerId)
        {
            return await c.Send<List<PlayerQueue>>(JustId(Commands.PlayerGroupVolumeDown, playerId, "player_id"));
        }
    }
}
