using Avalonia.Data.Converters;
using IconPacks.Avalonia.Material;
using System;
using System.Globalization;
using WateryTart.MusicAssistant.Models.Enums;

namespace WateryTart.Core.Converters
{
    public class RepeatModeToIconKindConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) 
                return null;

            var state = (RepeatMode)value;

            switch (state)

            {
                case RepeatMode.Off:
                    return PackIconMaterialKind.RepeatOff;

                case RepeatMode.All:
                    return PackIconMaterialKind.Repeat;

                case RepeatMode.One:
                    return PackIconMaterialKind.RepeatOnce;

                case RepeatMode.Unknown:
                    break;
            }

            return RepeatMode.Off;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
