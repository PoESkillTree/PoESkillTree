using System;
using System.Globalization;
using System.Windows.Data;

namespace PoESkillTree.Utils.Converter
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter : IValueConverter
    {
        public string TrueString { get; set; }

        public string FalseString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool) value;
            return b ? TrueString : FalseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (string) value;
            if (s == TrueString)
                return true;
            else if (s == FalseString)
                return false;
            else
                throw new ArgumentException("Value must be either TrueString of FalseString", nameof(value));
        }
    }
}