using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnitsNet;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.ViewModels.Popups
{
    public class TrackInfoViewModel : IPopupViewModel
    {
        public string Message => throw new NotImplementedException();

        public string? Title => throw new NotImplementedException();

        public Streamdetails? Streamdetails { get; }
        public QueuedItem Item { get; }

        public TrackInfoViewModel(QueuedItem item)
        {
            this.Streamdetails = item.StreamDetails;
            Item = item;
        }
    }

    public class CodecTypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {

            var format = GetFormat((string)value);
            if (string.IsNullOrEmpty(format))
                return null;


            return new Bitmap(AssetLoader.Open(new Uri("avares://WateryTart.Core/Assets/mediaassistant/flac.png")));

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
            else if (contentType.Contains("alac"))
                return "alac";

            return string.Empty;

        }
    }
}
