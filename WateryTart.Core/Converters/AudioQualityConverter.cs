using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.Service.MassClient.Models;

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
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats the sample rate as kHz (e.g., 44.1kHz, 48kHz)
/// </summary>
public class AudioFormatSampleRateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AudioFormat audioFormat && audioFormat.SampleRate > 0)
        {
            double sampleRateKhz = audioFormat.SampleRate / 1000.0;
            return $"{sampleRateKhz:F1}kHz";
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Formats the bit depth (e.g., 16bit, 24bit)
/// </summary>
public class AudioFormatBitDepthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AudioFormat audioFormat && audioFormat.BitDepth > 0)
        {
            return $"{audioFormat.BitDepth}bit";
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}