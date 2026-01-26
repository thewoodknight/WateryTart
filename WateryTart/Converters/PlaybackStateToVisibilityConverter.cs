using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.MassClient.Models;

namespace WateryTart.Converters;

public class PlaybackStateToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PlaybackState)
        {
            var state = (PlaybackState)value;
            if (state == PlaybackState.playing)
                return true;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}