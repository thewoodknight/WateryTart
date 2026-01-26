using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace WateryTart.Converters;

public class SecondsToMinutesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int seconds = (int)value;

        var span = new TimeSpan(0, 0, seconds);
        return string.Format("{0}:{1:00}", (int)span.TotalMinutes, span.Seconds);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}