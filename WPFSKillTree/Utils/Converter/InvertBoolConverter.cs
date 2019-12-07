using System;
using System.Globalization;
using System.Windows.Data;

namespace PoESkillTree.Utils.Converter
{
    /// <summary>
    /// Converter that inverts boolean values.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(value is bool b) || !b;
        }
    }
}