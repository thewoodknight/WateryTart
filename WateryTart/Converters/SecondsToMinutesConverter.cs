using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WateryTart.MassClient.Models;

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
        throw new NotImplementedException();
    }
}


public class ArtistsToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        IList<Artist> artists = (IList<Artist>)value;

        StringBuilder sb = new StringBuilder();
        foreach (var a in artists)
            sb.Append(a.Name);

        return sb.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}