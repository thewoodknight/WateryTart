using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.Core.Services;

namespace WateryTart.Core.Converters;
public class ColourPickConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var x = (ColourChosen)value;
        var y = parameter as string;

        switch (x)
        {
            case ColourChosen.AB when y == "AB":
            case ColourChosen.CD when y == "CD":
                return true;
            default:
                return false;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}