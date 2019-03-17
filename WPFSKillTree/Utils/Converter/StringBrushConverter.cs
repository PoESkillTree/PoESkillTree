using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PoESkillTree.Utils.Converter
{
    /// <summary>
    /// Value converter between color string and Brush.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StringBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts color string to Brush.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;

            // No color.
            if (string.IsNullOrEmpty(str)) return null;

            // Hexadecimal format.
            if (str.StartsWith("#"))
                return new SolidColorBrush(ColorUtils.FromRgbString(str));

            throw new ArgumentException("Invalid color string: " + str);
        }

        /// <summary>
        /// Converts Brush to color string.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = value as SolidColorBrush;

            // No brush.
            if (brush == null) return null;

            return brush.Color.ToString();
        }
    }
}
