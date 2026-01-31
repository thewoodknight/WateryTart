using System.Collections;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    extension(IMassWsClient c)
    {
        public async Task<CountResponse> AlbumsCountAsync()
        {
            var m = new Message("music/albums/count")
            {
                args = new Hashtable
                {
                    { "favorite_only", "false" },
                    { "album_types", "[\"album\", \"single\", \"live\", \"soundtrack\", \"compilation\", \"ep\", \"unknown\"]" }
                }
            };

            return await SendAsync<CountResponse>(c, m);
        }

        public async Task<CountResponse> TrackCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/tracks/count", "false", "favourite_only"));
        }

        public async Task<PlaylistResponse> PlaylistGetAsync(string playlistId, string provider_instance_id_or_domain)
        {
            return await SendAsync<PlaylistResponse>(c, IdAndProvider(Commands.PlaylistGet, playlistId, provider_instance_id_or_domain));
        }

        public async Task<TracksResponse> PlaylistTracksGetAsync(string playlistId, string provider_instance_id_or_domain)
        {
            return await SendAsync<TracksResponse>(c, IdAndProvider(Commands.PlaylistTracksGet, playlistId, provider_instance_id_or_domain));
        }

        public async Task<AlbumsResponse> MusicAlbumsLibraryItemsAsync()
        {
            return await SendAsync<AlbumsResponse>(c, JustCommand(Commands.MusicAlbumLibraryItems));
        }

        public async Task<AlbumResponse> MusicAlbumGetAsync(string id, string provider_instance_id_or_domain)
        {
            return await SendAsync<AlbumResponse>(c, IdAndProvider(Commands.MusicAlbumGet, id, provider_instance_id_or_domain));
        }

        public async Task<TracksResponse> MusicAlbumTracksAsync(string id, string provider_instance_id_or_domain)
        {
            return await SendAsync<TracksResponse>(c, IdAndProvider(Commands.MusicAlbumTracks, id, provider_instance_id_or_domain));
        }

        public async Task<RecommendationResponse> MusicRecommendationsAsync()
        {
            return await SendAsync<RecommendationResponse>(c, JustCommand(Commands.MusicRecommendations));
        }
    }
}