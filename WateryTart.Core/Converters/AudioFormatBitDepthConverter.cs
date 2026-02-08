using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

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
        return value;
    }
}