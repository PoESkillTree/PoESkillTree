using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace POESKillTree.Utils.Converter
{
    /// <summary>
    /// Converter used to not hide DataGridTemplateColumn contents in empty new row placeholders
    /// that are added to the bottom of the DataGrid.
    /// </summary>
    public class IsNamedObjectVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.GetType().Name == "NamedObject" ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}