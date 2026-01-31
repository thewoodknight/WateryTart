namespace WateryTart.Service.MassClient.Messages
{
    public static partial class Commands
    {
        public static string PlayersAll = "players/all";
        public static string PlayerQueuePlayMedia = "player_queues/play_media";
        public static string PlayerQueuesAll = "player_queues/all";
        public static string PlayerQueueItems = "player_queues/items";
        public static string PlayerActiveQueue = "player_queues/get_active_queue";

        public static string PlaylistGet = "music/playlists/get";
        public static string PlaylistTracksGet = "music/playlists/playlist_tracks";

        public static string PlayerNext = "players/cmd/next";
        public static string PlayerPlay = "players/cmd/play";
        public static string PlayerPlayPause = "players/cmd/play_pause";
        public static string PlayerPrevious = "players/cmd/previous";

        public static string PlayerGroupVolumeUp = "players/cmd/group_volume_up";
        public static string PlayerGroupVolumeDown = "players/cmd/group_volume_down";
        public static string PlayerGroupVolume = "players/cmd/group_volume";
    }
}
