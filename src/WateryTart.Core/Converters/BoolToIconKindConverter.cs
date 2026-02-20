using Avalonia.Data.Converters;
using IconPacks.Avalonia.Material;
using System;
using System.Globalization;

namespace WateryTart.Core.Converters;

public class BoolToIconKindConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Parameter format: "FalseKind,TrueKind"
        var kinds = (parameter as string)?.Split(',') ?? Array.Empty<string>();
        var falseKind = kinds.Length > 0 ? kinds[0] : nameof(PackIconMaterialKind.HeartOutline);
        var trueKind = kinds.Length > 1 ? kinds[1] : nameof(PackIconMaterialKind.Heart);

        var isTrue = value is bool b && b;
        var kindName = isTrue ? trueKind : falseKind;

        // Try to parse the PackIconMaterialKind enum
        if (Enum.TryParse(typeof(PackIconMaterialKind), kindName, out var result))
            return result!;

        // Fallback
        return PackIconMaterialKind.HeartOutline;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
