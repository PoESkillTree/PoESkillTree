using System;
using System.Globalization;
using System.Windows.Data;

namespace POESKillTree.Utils.Converter
{
    /// <summary>
    /// Converts Enum values to their descriptions (Description attribute).
    /// </summary>
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

    /// <summary>
    /// Converts Enum values to booleans and back.
    /// E.g. to use an Enum as Binding to a group of radio buttons
    /// (each radio button represents a value of the Enum).
    /// </summary>
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

    /// <summary>
    /// Converts flag Enums to booleans and back.
    /// E.g. to use an Enum as Binding to a group of check boxes
    /// (each check box represents a Flag in the Enum).
    /// </summary>
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