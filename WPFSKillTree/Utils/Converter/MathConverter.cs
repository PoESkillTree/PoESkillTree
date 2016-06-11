using System;
using System.Globalization;
using System.Windows.Data;

namespace POESKillTree.Utils.Converter
{
    [ValueConversion(typeof(double), typeof(double))]
    public class SubtractionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Max((double) value - (double) parameter, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}