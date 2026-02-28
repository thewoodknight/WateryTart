using System;
using System.Globalization;
using Avalonia.Data.Converters;
using WateryTart.Core.Services;

namespace WateryTart.Core.Converters;

#pragma warning disable CRRSP08 
public class ColourPickConverter : IValueConverter
#pragma warning restore CRRSP08
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var x = (ColourChosen)value!;
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
        return value;
    }
}