using Autofac;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Linq;
using WateryTart.Core.Services;
using WateryTart.MusicAssistant.Models;
namespace WateryTart.Core.ViewModels.Popups
{
    public partial class TrackInfoViewModel : ReactiveObject, IPopupViewModel
    {
        public string Message => throw new NotImplementedException();

        public string? Title => throw new NotImplementedException();

        public Streamdetails? StreamDetails { get; }
        public QueuedItem Item { get; }

        [Reactive] public partial ProviderManifest Provider { get; set; } = null!;
        [Reactive] public partial string InputProviderIcon { get; set; } = string.Empty;
        [Reactive] public partial Player Player { get; set; }
        [Reactive] public partial ProviderManifest? PlayerProvider { get; set; }

        [Reactive] public partial string OutputProviderIcon { get; set; } = string.Empty;
        [Reactive] public partial string QualityString { get; set; } = string.Empty;

        public TrackInfoViewModel(QueuedItem item, Player player)
        {
            this.StreamDetails = item.StreamDetails;
            Item = item;
            Player = player;

            if (StreamDetails == null)
                return;

            Provider = GetProvider(Item.MediaItem!.ProviderMappings![0].ProviderDomain!);
            if (!string.IsNullOrEmpty(Provider!.IconSvgDark))
                InputProviderIcon = Provider.IconSvgDark;
            else InputProviderIcon = Provider.IconSvg;

            double sampleRateKhz = StreamDetails.AudioFormat!.SampleRate / 1000.0;
            QualityString = $"{sampleRateKhz:F1}kHz / {StreamDetails?.AudioFormat.BitDepth}bit";

            PlayerProvider = GetProvider(Player!.Provider!);

            if (!string.IsNullOrEmpty(PlayerProvider?.IconSvgDark))
                OutputProviderIcon = PlayerProvider.IconSvgDark;
            else if (!string.IsNullOrEmpty(PlayerProvider?.IconSvg))
                OutputProviderIcon = PlayerProvider.IconSvg;
            else if (!string.IsNullOrEmpty(PlayerProvider?.IconSvgMonochrome))
                OutputProviderIcon = PlayerProvider.IconSvgMonochrome;
        }

        private static ProviderManifest? GetProvider(string domain)
        {
            var providerService = App.Container.Resolve<ProviderService>();
            var p = providerService.ProviderManifests.FirstOrDefault(x => x.Domain == domain);

            return p;
        }
    }
}
