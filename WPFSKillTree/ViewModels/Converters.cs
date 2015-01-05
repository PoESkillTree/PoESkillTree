using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace POESKillTree.ViewModels
{
    class SignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is float)
            {
                float val = (float)value;
                if (targetType == typeof(Brush))
                {
                    if (val == 0)
                        return Brushes.White;

                    if (val > 0)
                        return Brushes.Green;

                    if (val < 0)
                        return Brushes.Red;
                }
                if (targetType == typeof(Color))
                {
                    if (val == 0)
                        return Colors.White;

                    if (val > 0)
                        return Colors.Green;

                    if (val < 0)
                        return Colors.Red;
                }
                else if (targetType == typeof(Visibility))
                {
                    if (val == 0)
                        return Visibility.Collapsed;

                    return Visibility.Visible;
                }
                else if (targetType == typeof(int))
                {
                    if (val == 0)
                        return 0;

                    if (val > 0)
                        return 1;

                    if (val < 0)
                        return -1;
                }

                throw new NotImplementedException("cannot convert to target type '" + targetType.Name + "'");
            }

            throw new NotImplementedException("can convert only from floats");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ArrayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is Array)
            {
                bool allzeros = false;
                if(value is float[])
                    allzeros = ((float[])value).All(v=>v==0f);
                else if (value is int[])
                    allzeros = ((int[])value).All(v => v == 0f);
                else if (value is double[])
                    allzeros = ((double[])value).All(v => v == 0.0);

                if (allzeros)
                    return Visibility.Collapsed;

                if (((Array)value).Length != 0)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
