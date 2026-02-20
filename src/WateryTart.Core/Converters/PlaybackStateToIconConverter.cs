using Avalonia.Data.Converters;
using IconPacks.Avalonia.Material;
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
                PlaybackState.Playing => PackIconMaterialKind.Pause,
                PlaybackState.Paused => PackIconMaterialKind.Play,
                PlaybackState.Idle => PackIconMaterialKind.Play,
                _ => PackIconMaterialKind.Play
            };
        }

        // Fallback: handle string values
        if (value is string state)
        {
            return state?.ToLowerInvariant() switch
            {
                "playing" => PackIconMaterialKind.Pause,
                "paused" => PackIconMaterialKind.Play,
                "stopped" => PackIconMaterialKind.Play,
                "idle" => PackIconMaterialKind.Play,
                _ => PackIconMaterialKind.Play
            };
        }

        return PackIconMaterialKind.Play;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}