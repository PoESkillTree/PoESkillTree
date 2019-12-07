using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils.Converter
{
    /// <summary>
    /// Converts Enum values to their descriptions (Description attribute).
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum e)
            {
                return e.GetDescription() ?? e.ToString();
            }
            return "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts Enum values to their descriptions if they have one.
    /// If they don't, they are converted to their .toString() with spaces in front
    /// of each word.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(string))]
    public class EnumToSpacedStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Enum e)
            {
                return e.GetDescription() ?? Regex.Replace(e.ToString(), @"([a-z])([A-Z])", "$1 $2");
            }
            return "";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
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
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(parameter) ?? (parameter is null);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b ? parameter : Binding.DoNothing;
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

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var mask = (int) (parameter ?? 0);
            _target = (int) (value ?? 0);
            return (mask & _target) != 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            _target ^= (int) (parameter ?? 0);
            return Enum.Parse(targetType, _target.ToString());
        }
    }
}