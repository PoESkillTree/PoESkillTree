using System;
using System.Globalization;
using System.Windows.Data;

namespace POESKillTree.Utils.Converter
{
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var e = value as Enum;
            if (e != null)
            {
                return e.GetDescription();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(Enum), typeof(bool))]
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Enum), typeof(bool))]
    public class FlagsToBoolConverter : IValueConverter
    {
        private int _target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mask = (int) parameter;
            _target = (int) value;
            return (mask & _target) != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _target ^= (int) parameter;
            return Enum.Parse(targetType, _target.ToString());
        }
    }
}