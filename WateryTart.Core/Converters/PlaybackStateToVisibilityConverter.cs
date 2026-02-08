using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Converters;

public class PlaybackStateToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PlaybackState state) 
            return false;

        return state == PlaybackState.Playing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}