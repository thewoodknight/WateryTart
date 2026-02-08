using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.MusicAssistant.Models;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Converters;

public class HomeExtraToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var item = (Item)value;
        string output = string.Empty;
        switch (item.MediaType)
        {
            case MediaType.Artist:
            case MediaType.Playlist:
                output = item.Owner ?? string.Empty;
                break;
            case MediaType.Album:
            case MediaType.Track:
                {
                    if (item.Artists != null && item.Artists.Count > 0)
                        output = item?.Artists?[0].Name ?? string.Empty;
                    break;
                }
            default:
                break;
        }

        return output;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}