using Newtonsoft.Json;
using WateryTart.MassClient.Messages;
using WateryTart.MassClient.Models;
using WateryTart.MassClient.Responses;

namespace WateryTart.MassClient;

public static class ClientExtensions
{

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

    extension(IMassWsClient c)
    {
        public void GetAuthToken(string username, string password, Action<AuthResponse> responseHandler)
        {
            c.Send<AuthResponse>(AuthMessages.Login(username, password), Deserialise<AuthResponse>(responseHandler), true);
        }

        public void ArtistGet(string id, string provider, Action<ArtistResponse> responseHandler)
        {
            c.Send<ArtistResponse>(ArtistMessages.ArtistGet(id, provider), Deserialise<ArtistResponse>(responseHandler));
        }

        public void ArtistsGet(Action<ArtistsResponse> responseHandler)
        {
            c.Send<ArtistsResponse>(ArtistMessages.ArtistsGet, Deserialise<ArtistsResponse>(responseHandler));
        }

        public void ArtistAlbums(string id, string provider, Action<AlbumsResponse> responseHandler)
        {
            c.Send<AlbumsResponse>(ArtistMessages.ArtistAlbums(id, provider), Deserialise<AlbumsResponse>(responseHandler));
        }

        public void ArtistCount(Action<CountResponse> responseHandler)
        {
            c.Send<CountResponse>(ArtistMessages.ArtistCount(), Deserialise<CountResponse>(responseHandler));
        }

        public void AlbumsCount(Action<CountResponse> responseHandler)
        {
            c.Send<CountResponse>(MusicMessages.AlbumsCount(), Deserialise<CountResponse>(responseHandler));
        }

        public void TrackCount(Action<CountResponse> responseHandler)
        {
            c.Send<CountResponse>(MusicMessages.TrackCount(), Deserialise<CountResponse>(responseHandler));
        }

        public void Play(string queueID, MediaItemBase t, PlayMode mode, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerQueuePlayMedia(queueID, t, mode), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerNext(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerNext(playerId), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPlay(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerPlay(playerId), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPlayPause(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerPlayPause(playerId), Deserialise<PlayersQueuesResponse>(responseHandler));
        }
        public void PlayerPrevious(string playerId, Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerPrevious(playerId), Deserialise<PlayersQueuesResponse>(responseHandler));
        }


        public void PlayersAll(Action<PlayerResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayersAll, Deserialise<PlayerResponse>(responseHandler));
        }

        public void PlayerQueuesAll(Action<PlayersQueuesResponse> responseHandler)
        {
            c.Send<PlayerResponse>(PlayerMessages.PlayerQueuesAll, Deserialise<PlayersQueuesResponse>(responseHandler));
        }

        public void PlayerActiveQueue(string id, Action<PlayerQueueResponse> responseHandler)
        {
            c.Send<PlayerQueueResponse>(PlayerMessages.PlayerActiveQueue(id), Deserialise<PlayerQueueResponse>(responseHandler));
        }



        public void PlaylistGet(string id, string provider, Action<PlaylistResponse> responseHandler)
        {
            c.Send<PlaylistResponse>(PlaylistMessages.PlaylistGet(id, provider), Deserialise<PlaylistResponse>(responseHandler));
        }

        public void PlaylistTracksGet(string id, string provider, Action<TracksResponse> responseHandler)
        {
            c.Send<TracksResponse>(PlaylistMessages.PlaylistTracksGet(id, provider), Deserialise<TracksResponse>(responseHandler));
        }

        public void MusicAlbumsLibraryItems(Action<AlbumsResponse> responseHandler)
        {
            c.Send<AlbumsResponse>(MusicMessages.MusicAlbumLibraryItems, Deserialise<AlbumsResponse>(responseHandler));
        }

        public void MusicAlbumGet(string id, string provider, Action<AlbumResponse> responseHandler)
        {
            c.Send<AlbumResponse>(MusicMessages.MusicAlbumGet(id, provider), Deserialise<AlbumResponse>(responseHandler));
        }

        public void MusicAlbumTracks(string id, string provider, Action<TracksResponse> responseHandler)
        {
            c.Send<TracksResponse>(MusicMessages.MusicAlbumTracks(id, provider), Deserialise<TracksResponse>(responseHandler));
        }

        public void MusicAlbumTracks(string id, Action<TracksResponse> responseHandler)
        {
            c.Send<TracksResponse>(MusicMessages.MusicAlbumTracks(id), Deserialise<TracksResponse>(responseHandler));
        }

        public void MusicRecommendations(Action<RecommendationResponse> responseHandler)
        {
            c.Send<RecommendationResponse>(MusicMessages.MusicRecommendations, Deserialise<RecommendationResponse>(responseHandler));
        }
    }
}