using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.Converters;

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