using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters
{
    public class TitleToMarginConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var title = value as string;
            return string.IsNullOrWhiteSpace(title)
                ? new Thickness(10, -20, 10, 0)
                : new Thickness(10, 0, 10, 0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
