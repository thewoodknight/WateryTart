using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

public class MetadataImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var item = value as MediaItemBase;
            if (item == null)
                return null;

            //If it is an item, but has a "image" field set, use that
            if (item.Image != null && !string.IsNullOrEmpty(item.Image.Path))
                //If the image field starts with http, use that
                if (item.Image.Provider != null)
                    return item.Image.Path.StartsWith(("http"))
                        ? item.Image.Path
                        : ProxyString(item.Image.Path, item.Image.Provider);

            //If there is no image field set, use metadata, make sure its not null
            if (item.Metadata?.Images == null)
                return null;

            //Try a locally accessible source first
            var result = item.Metadata.Images.FirstOrDefault(i => !i.RemotelyAccessible);
            if (result == null)
                result = item.Metadata.Images.FirstOrDefault(i => i.RemotelyAccessible);

            if (result?.Provider != null)
                if (result.Path != null)
                    return result.Path != null && result.Path.StartsWith("http")
                        ? result.Path
                        : ProxyString(result.Path, result.Provider);
        }
        catch (Exception ex)
        {
            App.Logger?.LogError(ex, "image metadata failure");
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }

    private static string ProxyString(string path, string provider)
    {
        return $"http://{App.BaseUrl}/imageproxy?path={Uri.EscapeDataString(path)}&provider={provider}&checksum=&size=256";
    }
}