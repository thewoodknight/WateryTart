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

    public static void GetAuthToken(this IMassWSClient c, string username, string password, Action<AuthResponse> responseHandler)
    {
        c.Send<AuthResponse>(AuthMessages.Login(username, password), Deserialise<AuthResponse>(responseHandler));
    }

    public static void Play(this IMassWSClient c, string queueID, Item t, Action<PlayerQueueResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayerQueuePlayMedia(queueID, t), Deserialise<PlayerQueueResponse>(responseHandler));
    }

    public static void PlayersAll(this IMassWSClient c, Action<PlayerResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayersAll, Deserialise<PlayerResponse>(responseHandler));
    }

    public static void PlayerQueuesAll(this IMassWSClient c, Action<PlayerQueueResponse> responseHandler)
    {
        c.Send<PlayerResponse>(PlayerMessages.PlayerQueuesAll, Deserialise<PlayerQueueResponse>(responseHandler));
    }

    public static void MusicAlbumsLibraryItems(this IMassWSClient c, Action<AlbumsResponse> responseHandler)
    {
        c.Send<AlbumsResponse>(MusicMessages.MusicAlbumLibraryItems, Deserialise<AlbumsResponse>(responseHandler));
    }

    public static void MusicAlbumGet(this IMassWSClient c, string id, Action<TracksResponse> responseHandler)
    {
        c.Send<TracksResponse>(MusicMessages.MusicAlbumTracks(id), Deserialise<TracksResponse>(responseHandler));
    }
}