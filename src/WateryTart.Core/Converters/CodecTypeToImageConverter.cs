using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
namespace WateryTart.Core.Converters
{
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
