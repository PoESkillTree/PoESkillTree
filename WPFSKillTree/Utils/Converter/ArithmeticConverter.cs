using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PoESkillTree.Utils.Converter
{
    /// <summary>
    /// Converts values by calculating the sum of them, <see cref="Constant"/> and the converter parameter
    /// (if given). ConvertBack is only implemented for single value conversion.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class SumConverter : IMultiValueConverter, IValueConverter
    {
        public double Minimum { private get; set; } = double.NegativeInfinity;

        public double Constant { private get; set; }

        public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            double sum = values.Sum(System.Convert.ToDouble);
            if (parameter != null)
            {
                sum += System.Convert.ToDouble(parameter);
            }
            return Math.Max(sum + Constant, Minimum);
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Convert(new[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double sum = System.Convert.ToDouble(value);
            if (parameter != null)
            {
                sum -= System.Convert.ToDouble(parameter);
            }
            return sum - Constant;
        }
    }

    /// <summary>
    /// Converts values by calculating the product of them, <see cref="Constant"/> and the converter parameter
    /// (if given). ConvertBack is only implemented for single value conversion.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class ProductConverter : IMultiValueConverter, IValueConverter
    {
        public double Constant { private get; set; } = 1;

        public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            double product = Constant;
            foreach (var value in values)
            {
                product *= System.Convert.ToDouble(value);
            }
            if (parameter != null)
            {
                product *= System.Convert.ToDouble(parameter);
            }
            return product;
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Convert(new[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            double product = System.Convert.ToDouble(value);
            if (parameter != null)
            {
                product /= System.Convert.ToDouble(parameter);
            }
            return product / Constant;
        }
    }
}