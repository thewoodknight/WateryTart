using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters;

public class MassImageImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Image? image = (Image)value;

        if (image == null)
            return string.Empty;

        if (image.RemotelyAccessible)
            return image.Path;
        else if (image.Path != null)
            if (image.Provider != null)
                return ProxyString(image.Path, image.Provider);

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }

    private static string ProxyString(string path, string provider)
    {
        return string.Format("http://{0}/imageproxy?path={1}&provider={2}&checksum=&size=256", App.BaseUrl, Uri.EscapeDataString(path), provider);
    }
}