using Avalonia.Data.Converters;
using IconPacks.Avalonia.Material;
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
                    return PackIconMaterialKind.VolumeHigh;
                if (volume >= 33)
                    return PackIconMaterialKind.VolumeMedium;
                if (volume > 0)
                    return PackIconMaterialKind.VolumeLow;
                return PackIconMaterialKind.VolumeOff;
            }
        return PackIconMaterialKind.VolumeOff;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
