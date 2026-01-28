namespace WateryTart.MassClient.Messages;

public static class Commands
{
    public static string Auth = "auth";
    public static string AuthLogin = "auth/login";

    public static string ArtistAlbums = "music/artists/artist_albums";
    public static string ArtistTracks = "music/artists/artist_tracks";
    public static string ArtistGet = "music/artists/get";
    public static string ArtistsGet = "music/artists/library_items";
    public static string ArtistsCount = "music/artists/count";

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

    public static string MusicAlbumLibraryItems = "music/albums/library_items";
    public static string MusicAlbumGet = "music/albums/get";
    public static string MusicAlbumTracks = "music/albums/album_tracks";
    public static string MusicRecommendations = "music/recommendations";
}