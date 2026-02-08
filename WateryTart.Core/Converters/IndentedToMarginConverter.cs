using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace WateryTart.Core.Converters;

public class IndentedToMarginConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isIndented = (bool)value!;
        if (isIndented)
            return new Thickness(30, 0, 0, 0);
        return new Thickness(0, 0, 0, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}