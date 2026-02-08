using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient;

public static partial class MassClientExtensions
{
    extension(IMassWsClient c)
    {
        public async Task<ArtistResponse> ArtistGetAsync(string artistid, string provider_instance_id_or_domain)
        {
            return await SendAsync<ArtistResponse>(c, IdAndProvider(Commands.ArtistGet, artistid, provider_instance_id_or_domain));
        }

        public async Task<ArtistsResponse> ArtistsGetAsync()
        {
            return await SendAsync<ArtistsResponse>(c, JustCommand(Commands.ArtistsGet));
        }

        public async Task<AlbumsResponse> ArtistAlbumsAsync(string artistid, string provider_instance_id_or_domain)
        {
            return await SendAsync<AlbumsResponse>(c, IdAndProvider(Commands.ArtistAlbums, artistid, provider_instance_id_or_domain));
        }

        public async Task<CountResponse> ArtistCountAsync()
        {
            var m = new Message(Commands.ArtistsCount)
            {
                args = new Dictionary<string, object>()
                {
                    { "favorite_only", "false" },
                    { "album_artists_only", "true" }
                }
            };
            return await SendAsync<CountResponse>(c, m);
        }
    }
}