using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WateryTart.MusicAssistant;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.WsExtensions;

namespace WateryTart.Core.Services
{
    public class ProviderService(MusicAssistantClient massClient, ILoggerFactory loggerFactory)
    {
        public List<ProviderManifest> ProviderManifests { get; set; } = [];
        private readonly MusicAssistantClient _massClient = massClient;
        private readonly ILogger<ProviderService> logger = loggerFactory.CreateLogger<ProviderService>();

        public async Task Load()
        {
            var providers = await _massClient.WithWs().GetProvidersManifestsAsync();
            if (providers == null || providers.Result == null)
                return;

            ProviderManifests = providers.Result;
        }
    }
}
