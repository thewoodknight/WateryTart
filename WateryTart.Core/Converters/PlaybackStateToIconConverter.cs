using Avalonia.Data.Converters;
using Material.Icons;
using System;
using System.Globalization;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Converters;

public class PlaybackStateToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle PlaybackState enum
        if (value is PlaybackState playbackState)
        {
            return playbackState switch
            {
                PlaybackState.Playing => MaterialIconKind.Pause,
                PlaybackState.Paused => MaterialIconKind.Play,
                PlaybackState.Idle => MaterialIconKind.Play,
                _ => MaterialIconKind.Play
            };
        }

        // Fallback: handle string values
        if (value is string state)
        {
            return state?.ToLowerInvariant() switch
            {
                "playing" => MaterialIconKind.Pause,
                "paused" => MaterialIconKind.Play,
                "stopped" => MaterialIconKind.Play,
                "idle" => MaterialIconKind.Play,
                _ => MaterialIconKind.Play
            };
        }

        return MaterialIconKind.Play;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}