using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using WateryTart.MusicAssistant.Models;

namespace WateryTart.Core.Converters
{
    // MultiValue converter: [Available (bool), SelectedPlayer (Player), CurrentPlayerId (string)] -> bool IsVisible
    public class PlayerVisibleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (values == null || values.Count < 3)
                    return false;

                var availableObj = values[0];
                var selectedObj = values[1];
                var currentIdObj = values[2];

                var available = availableObj is bool b && b;
                var currentId = currentIdObj as string;

                if (!available)
                    return false;

                if (selectedObj is Player sel && !string.IsNullOrEmpty(currentId))
                {
                    return !string.Equals(sel.PlayerId, currentId, StringComparison.Ordinal);
                }

                // No selected player or cannot determine, fall back to available
                return available;
            }
            catch
            {
                return false;
            }
        }

        public object[]? ConvertBack(object? value, Type[]? targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
