using System.Collections.Generic;
using System.Threading.Tasks;
using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    // ✅ FIXED: Proper extension method syntax
    public static async Task<RecommendationResponse> MusicRecommendationsAsync(this IMassWsClient c)
    {
        return await SendAsync<RecommendationResponse>(c, JustCommand(Commands.MusicRecommendations));
    }

    extension(IMassWsClient c)
    {
        public async Task<CountResponse> AlbumsCountAsync()
        {
            var m = new Message("music/albums/count")
            {
                args = new Dictionary<string, object>()
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

        public async Task<TracksResponse> TracksGetAsync(int? limit = null, int? offset = null)
        {
            var m = new Message("music/tracks/library_items")
            {
                args = new Dictionary<string, object>()
                {
                    { "favorite_only", "false" },
                }
            };
            if (limit.HasValue)
            {
                m.args["limit"] = limit.Value.ToString();
            }
            if (offset.HasValue)
            {
                m.args["offset"] = offset.Value.ToString();
            }
            return await SendAsync<TracksResponse>(c, m);
        }

        public async Task<CountResponse> PlaylistsCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/playlists/count", "false", "favorite_only"));
        }

        public async Task<CountResponse> AudiobookCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/audiobooks/count", "false", "favorite_only"));
        }

        public async Task<CountResponse> PodcastCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/podcasts/count", "false", "favorite_only"));
        }

        public async Task<CountResponse> GenreCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/genre/count", "false", "favorite_only"));
        }
        public async Task<CountResponse> RadiosCountAsync()
        {
            return await SendAsync<CountResponse>(c, JustId("music/radios/count", "false", "favorite_only"));
        }

        public async Task<PlaylistResponse> PlaylistGetAsync(string playlistId, string provider_instance_id_or_domain)
        {
            return await SendAsync<PlaylistResponse>(c, IdAndProvider(Commands.PlaylistGet, playlistId, provider_instance_id_or_domain));
        }

        public async Task<PlaylistsResponse> PlaylistsGetAsync(int? limit = null, int? offset = null)
        {
            var m = new Message("music/playlists/library_items")
            {
                args = new Dictionary<string, object>()
                {
                    { "favorite_only", "false" },

                }
            };

            if (limit.HasValue)
            {
                m.args["limit"] = limit.Value.ToString();
            }
            if (offset.HasValue)
            {
                m.args["offset"] = offset.Value.ToString();
            }

            return await SendAsync<PlaylistsResponse>(c, m);
        }

        public async Task<TracksResponse> PlaylistTracksGetAsync(string playlistId, string provider_instance_id_or_domain)
        {
            return await SendAsync<TracksResponse>(c, IdAndProvider(Commands.PlaylistTracksGet, playlistId, provider_instance_id_or_domain));
        }

        public async Task<ArtistsResponse> ArtistsGetAsync(int? limit = null, int? offset = null)
        {
            var m = new Message("music/artists/library_items")
            {
                args = new Dictionary<string, object>()
                {
                    { "favorite_only", "false" },
                }
            };
            
            if (limit.HasValue)
            {
                m.args["limit"] = limit.Value.ToString();
            }
            if (offset.HasValue)
            {
                m.args["offset"] = offset.Value.ToString();
            }
            
            return await SendAsync<ArtistsResponse>(c, m);
        }

        public async Task<AlbumsResponse> MusicAlbumsLibraryItemsAsync(int? limit = null, int? offset = null)
        {
            var m = new Message(Commands.MusicAlbumLibraryItems)
            {
                args = new Dictionary<string, object>()
            };
            
            if (limit.HasValue)
            {
                m.args["limit"] = limit.Value.ToString();
            }
            if (offset.HasValue)
            {
                m.args["offset"] = offset.Value.ToString();
            }
            
            return await SendAsync<AlbumsResponse>(c, m);
        }

        public async Task<AlbumResponse> MusicAlbumGetAsync(string id, string provider_instance_id_or_domain)
        {
            return await SendAsync<AlbumResponse>(c, IdAndProvider(Commands.MusicAlbumGet, id, provider_instance_id_or_domain));
        }

        public async Task<TracksResponse> MusicAlbumTracksAsync(string id, string provider_instance_id_or_domain)
        {
            return await SendAsync<TracksResponse>(c, IdAndProvider(Commands.MusicAlbumTracks, id, provider_instance_id_or_domain));
        }
    }
}