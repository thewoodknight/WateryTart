using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

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
        return value;
    }
}