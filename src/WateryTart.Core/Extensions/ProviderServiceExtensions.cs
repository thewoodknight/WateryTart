using WateryTart.Core.Services;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Extensions
{
    public static class ProviderServiceExtensions
    {
        public static ProviderManifest? GetProvider(this ProviderService service, string domain)
        {
            return service.ProviderManifests.Find(p => p.Domain == domain);
        }
    }
}
