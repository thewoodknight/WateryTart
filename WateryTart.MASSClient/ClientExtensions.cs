using Newtonsoft.Json;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;

namespace WateryTart.MassClient;

public static partial class ClientExtensions
{
    private static Action<string> Deserialise<T>(Action<T> responseHandler)
    {
        Action<string> d = (r) =>
        {
            var y = JsonConvert.DeserializeObject<T>(r);
            responseHandler(y);
        };

        return d;
    }

    public static void GetAuthToken(this IMassWsClient c, string username, string password, Action<AuthResponse> responseHandler)
    {
        c.Send<AuthResponse>(AuthMessages.Login(username, password), Deserialise<AuthResponse>(responseHandler));
    }

    public static void ArtistGet(this IMassWsClient c, string id, string provider, Action<ArtistResponse> responseHandler)
    {
        c.Send<ArtistResponse>(ArtistMessages.ArtistGet(id, provider), Deserialise<ArtistResponse>(responseHandler));
    }
    public static void ArtistsGet(this IMassWsClient c, Action<ArtistsResponse> responseHandler)
    {
        c.Send<ArtistsResponse>(ArtistMessages.ArtistsGet, Deserialise<ArtistsResponse>(responseHandler));
    }


    
    public static void ArtistAlbums(this IMassWsClient c, string id, string provider, Action<AlbumsResponse> responseHandler)
    {
        c.Send<AlbumsResponse>(ArtistMessages.ArtistAlbums(id, provider), Deserialise<AlbumsResponse>(responseHandler));
    }

    public static void ArtistCount(this IMassWsClient c, Action<CountResponse> responseHandler)
    {
        c.Send<CountResponse>(ArtistMessages.ArtistCount(), Deserialise<CountResponse>(responseHandler));
    }


    public static void AlbumsCount(this IMassWsClient c, Action<CountResponse> responseHandler)
    {
        c.Send<CountResponse>(MusicMessages.AlbumsCount(), Deserialise<CountResponse>(responseHandler));
    }

    public static void TrackCount(this IMassWsClient c, Action<CountResponse> responseHandler)
    {
        c.Send<CountResponse>(MusicMessages.TrackCount(), Deserialise<CountResponse>(responseHandler));
    }

    public static void Play(this IMassWsClient c, string queueID, Item t, Action<PlayerQueueResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayerQueuePlayMedia(queueID, t), Deserialise<PlayerQueueResponse>(responseHandler));
    }

    public static void PlayersAll(this IMassWsClient c, Action<PlayerResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayersAll, Deserialise<PlayerResponse>(responseHandler));
    }

    public static void PlayerQueuesAll(this IMassWsClient c, Action<PlayerQueueResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayerQueuesAll, Deserialise<PlayerQueueResponse>(responseHandler));
    }


    public static void PlaylistGet(this IMassWsClient c, string id, string provider, Action<PlaylistResponse> responseHandler)
    {
        c.Send<PlaylistResponse>(PlaylistMessages.PlaylistGet(id, provider), Deserialise<PlaylistResponse>(responseHandler));
    }

    public static void PlaylistTracksGet(this IMassWsClient c, string id, string provider, Action<TracksResponse> responseHandler)
    {
        c.Send<TracksResponse>(PlaylistMessages.PlaylistTracksGet(id, provider), Deserialise<TracksResponse>(responseHandler));
    }

    public static void MusicAlbumsLibraryItems(this IMassWsClient c, Action<AlbumsResponse> responseHandler)
    {
        c.Send<AlbumsResponse>(MusicMessages.MusicAlbumLibraryItems, Deserialise<AlbumsResponse>(responseHandler));
    }
    public static void MusicAlbumGet(this IMassWsClient c, string id, string provider, Action<AlbumResponse> responseHandler)
    {
        c.Send<AlbumResponse>(MusicMessages.MusicAlbumGet(id, provider), Deserialise<AlbumResponse>(responseHandler));
    }

    public static void MusicAlbumTracks(this IMassWsClient c, string id, string provider, Action<TracksResponse> responseHandler)
    {
        c.Send<TracksResponse>(MusicMessages.MusicAlbumTracks(id, provider), Deserialise<TracksResponse>(responseHandler));
    }

    public static void MusicAlbumTracks(this IMassWsClient c, string id, Action<TracksResponse> responseHandler)
    {
        c.Send<TracksResponse>(MusicMessages.MusicAlbumTracks(id), Deserialise<TracksResponse>(responseHandler));
    }

    public static void MusicRecommendations(this IMassWsClient c, Action<RecommendationResponse> responseHandler)
    {
        c.Send<RecommendationResponse>(MusicMessages.MusicRecommendations, Deserialise<RecommendationResponse>(responseHandler));
    }

}