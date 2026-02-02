using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters;

public class FriendlyTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        TimeSpan ts;
        if (value is Int32)
            ts = TimeSpan.FromSeconds((Int32)value);
        else
            ts = TimeSpan.FromSeconds((double)value);
        return String.Format("{0}:{1:D2}", ts.Minutes, ts.Seconds);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}
