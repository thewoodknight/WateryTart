using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

/// <summary>
/// Extracts the codec type from AudioFormat (FLAC, MP3, OPUS, etc.)
/// </summary>
public class AudioFormatCodecConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AudioFormat audioFormat && !string.IsNullOrEmpty(audioFormat.ContentType))
        {
            var contentType = audioFormat.ContentType.ToUpper();
            
            // Extract codec from content_type (e.g., "audio/flac" -> "FLAC")
            if (contentType.Contains("FLAC"))
                return "FLAC";
            else if (contentType.Contains("MP3") || contentType.Contains("MPEG"))
                return "MP3";
            else if (contentType.Contains("OPUS"))
                return "OPUS";
            else if (contentType.Contains("AAC"))
                return "AAC";
            else if (contentType.Contains("VORBIS") || contentType.Contains("OGG"))
                return "OGG";
            else if (contentType.Contains("WAV"))
                return "WAV";
            else if (contentType.Contains("ALAC"))
                return "ALAC";
            
            // Fallback: try to extract after slash
            var parts = audioFormat.ContentType.Split('/');
            return parts.Length > 1 ? parts[1].ToUpper() : null;
        }
        
        return null;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}