using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;
using WateryTart.Core.ViewModels.Players;

namespace WateryTart.Core.Converters
{
    public class QualityToBackgroundBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush LowBrush = new SolidColorBrush(Color.Parse("#FFA500")); // Orange
        private static readonly SolidColorBrush GoodBrush = new SolidColorBrush(Color.Parse("#90EE90")); // LightGreen
        private static readonly SolidColorBrush HiresBrush = new SolidColorBrush(Color.Parse("#00FFFF")); // Cyan

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QualityTier qt)
            {
                return qt switch
                {
                    QualityTier.LOW => LowBrush,
                    QualityTier.HQ => GoodBrush,
                    QualityTier.HIRES => HiresBrush,
                    _ => LowBrush,
                };
            }

            if (value is string s && Enum.TryParse<QualityTier>(s, true, out var parsed))
                return Convert(parsed, targetType, parameter, culture);

            return LowBrush;
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
