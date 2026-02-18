using Avalonia.Data.Converters;
using Material.Icons;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters;

public class VolumeLevelToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
            if (value is int volume)
            {
                if (volume >= 66)
                    return MaterialIconKind.VolumeHigh;
                if (volume >= 33)
                    return MaterialIconKind.VolumeMedium;
                if (volume > 0)
                    return MaterialIconKind.VolumeLow;
                return MaterialIconKind.VolumeOff;
            }
        return MaterialIconKind.VolumeOff;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
