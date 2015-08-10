using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Converter used to not show DataGridTemplateColumns in empty new row placeholders,
    /// that are added to the bottom of the DataGrid.
    /// </summary>
    public class IsNamedObjectVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType().Name == "NamedObject")
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}