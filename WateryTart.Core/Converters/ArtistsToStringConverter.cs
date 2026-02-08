using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

public class ArtistsToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        IList<Artist> artists = (IList<Artist>)value!;

        StringBuilder sb = new StringBuilder();
        foreach (var a in artists)
            sb.Append(a.Name);

        return sb.ToString();
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}