using Avalonia.Data.Converters;
using System;
using System.Globalization;
using WateryTart.Service.MassClient.Models;
using WateryTart.Service.MassClient.Models.Enums;

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
                output = item.owner;
                break;
            case MediaType.Playlist:
                output = item.owner;
                break;
            case MediaType.Album:
            case MediaType.Track:
                {
                    if (item.artists != null && item.artists.Count > 0)
                        output = item?.artists?[0].Name ?? string.Empty;
                    break;
                }
            default:
                break;
        }

        return output;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}