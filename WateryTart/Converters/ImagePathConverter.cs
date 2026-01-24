using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace WateryTart.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //http://10.0.1.20:8095/imageproxy?path=%252Flibrary%252Fmetadata%252F26138%252Fthumb%252F1767975309&provider=plex--eShtiaj3&checksum=&size=256" style="">
            //http://10.0.1.20:8095/imageproxy?path=%2Flibrary%2Fmetadata%2F28073%2Fthumb%2F1761398032&provider=plex--eShtiaj3&checksum=&size=256
            string path = (string)value;

            return "http://10.0.1.20:8095/imageproxy?path=" + Uri.EscapeDataString(path) + "&provider=plex--eShtiaj3&checksum=&size=256";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class SecondsToMinutesConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            int seconds = (int)value;

            var span = new TimeSpan(0, 0, seconds);
            return string.Format("{0}:{1:00}", (int)span.TotalMinutes,span.Seconds);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
