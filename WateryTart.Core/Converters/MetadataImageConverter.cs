using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;
using WateryTart.Service.MassClient.Models;

namespace WateryTart.Core.Converters;

public class MetadataImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            MediaItemBase item;
            if (value is MediaItemBase)

                item = (MediaItemBase?)value;

            else
                return null; //string.Empty;

            //If it's not an item, return
            if (item == null)
                return null;

            //If it is an item, but has a "image" field set, use that
            if (item.image != null && !string.IsNullOrEmpty(item.image.path))
                //If the image field starts with http, use that
                return item.image.path.StartsWith(("http"))
                    ? item.image.path
                    : ProxyString(item.image.path, item.image.provider);

            //If there is no image field set, use metadata, make sure its not null
            if (item.Metadata.Images == null)
                return null;

            //Try a locally accessible source first
            var result = item.Metadata.Images.FirstOrDefault(i => i.remotely_accessible == false);
            if (result == null)
                result = item.Metadata.Images.FirstOrDefault(i => i.remotely_accessible);

            return result.path.StartsWith("http") ? result.path : ProxyString(result.path, result.provider);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string ProxyString(string path, string provider)
    {
        return string.Format("http://{0}/imageproxy?path={1}&provider={2}&checksum=&size=256", App.BaseUrl, Uri.EscapeDataString(path), provider);
    }
}