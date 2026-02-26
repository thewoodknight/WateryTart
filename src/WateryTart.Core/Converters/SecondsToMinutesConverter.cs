using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters;

public class SecondsToMinutesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;
        int seconds = (int)value!;

        var span = new TimeSpan(0, 0, seconds);
        return $"{(int)span.TotalMinutes}:{span.Seconds:00}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}