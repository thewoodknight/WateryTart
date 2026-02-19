using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters;

public class FriendlyTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        TimeSpan ts;
        if (value == null)
            return string.Empty;

        if (value is Int32 i)
            ts = TimeSpan.FromSeconds(i);
        else
            ts = TimeSpan.FromSeconds((double)value);
        return $"{ts.Minutes}:{ts.Seconds:D2}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }

}
