using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.Services
{
    public static class ProviderServiceExtensions
    {
        public static ProviderManifest? GetProvider(this ProviderService service, string domain)
        {
            return service.ProviderManifests.Find(p => p.Domain == domain);
        }
    }
    public class ProviderService
    {
        public List<ProviderManifest> ProviderManifests { get; set; } = new List<ProviderManifest>();
        private readonly MusicAssistantClient massClient;
        private readonly ILogger<ProviderService> logger;

        public ProviderService(MusicAssistantClient massClient, ILoggerFactory loggerFactory)
        {
            this.massClient = massClient;
            logger = loggerFactory.CreateLogger<ProviderService>();
        }

        public async Task Load()
        {
            var providers = await massClient.WithWs().GetProvidersManifestsAsync();
            if (providers == null || providers.Result == null)
                return;

            ProviderManifests = providers.Result;
        }
    }
}
