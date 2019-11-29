using System;
using System.Globalization;
using System.Windows.Data;

namespace PoESkillTree.Utils.Converter
{
    public class EmptyStringConverter : IValueConverter
    {
        public string? EmptyStringRepresentation { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var s = (string?) value;
            return string.IsNullOrEmpty(s) ? EmptyStringRepresentation : s;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var s = (string?) value;
            return s == EmptyStringRepresentation ? "" : s;
        }
    }
}