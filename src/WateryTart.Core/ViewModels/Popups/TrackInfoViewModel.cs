using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Sendspin.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;
using WateryTart.Core.Services;
using WateryTart.Core.Settings;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels.Popups
{
    public partial class TrackInfoViewModel : ReactiveObject, IPopupViewModel
    {
        public string Message => throw new NotImplementedException();

        public string? Title => throw new NotImplementedException();

        public Streamdetails? Streamdetails { get; }
        public QueuedItem Item { get; }

        [Reactive] public partial ProviderManifest Provider { get; set; }
        [Reactive] public partial string InputProviderIcon { get; set; }
        [Reactive] public partial Player Player { get; set; }
        [Reactive] public partial ProviderManifest PlayerProvider { get; set; }

        [Reactive] public partial string OutputProviderIcon { get; set; }
        [Reactive] public partial string QualityString { get; set; }
        public TrackInfoViewModel(QueuedItem item, Player player)
        {
            this.Streamdetails = item.StreamDetails;
            Item = item;
            Provider = GetProvider(Item.MediaItem.ProviderMappings[0].ProviderDomain);
            if (!string.IsNullOrEmpty(Provider.IconSvgDark))
                InputProviderIcon = Provider.IconSvgDark;
            else InputProviderIcon = Provider.IconSvg;

            Player = player;
            double sampleRateKhz = Streamdetails.AudioFormat.SampleRate / 1000.0;
            QualityString = $"{sampleRateKhz:F1}kHz / {Streamdetails?.AudioFormat.BitDepth}bit";

            PlayerProvider = GetProvider(Player.Provider);

            if (!string.IsNullOrEmpty(PlayerProvider.IconSvgDark))
                OutputProviderIcon = PlayerProvider.IconSvgDark;
            else if (!string.IsNullOrEmpty(PlayerProvider.IconSvg))
                OutputProviderIcon = PlayerProvider.IconSvg;
            else
                OutputProviderIcon = PlayerProvider.IconSvgMonochrome;
        }

        private ProviderManifest GetProvider(string domain)
        {
            var providerService = App.Container.GetRequiredService<ProviderService>();
            var p = providerService.ProviderManifests.FirstOrDefault(x => x.Domain == domain);

            return p;
        }
    }

    public class CodecTypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {

            var format = GetFormat((string)value);
            if (string.IsNullOrEmpty(format))
                return null;


            return new Bitmap(AssetLoader.Open(new Uri($"avares://WateryTart.Core/Assets/mediaassistant/{format}.png")));

        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        private static string GetFormat(string contentType)
        {
            contentType = contentType.ToLower();

            // Extract codec from content_type (e.g., "audio/flac" -> "FLAC")
            if (contentType.Contains("flac"))
                return "flac";
            else if (contentType.Contains("mp3") || contentType.Contains("mpeg"))
                return "mp3";
            else if (contentType.Contains("opus"))
                return "opus";
            else if (contentType.Contains("aac"))
                return "aac";
            else if (contentType.Contains("vorbis") || contentType.Contains("ogg"))
                return "ogg";
            else if (contentType.Contains("wav"))
                return "wav";

            return string.Empty;

        }
    }
}
