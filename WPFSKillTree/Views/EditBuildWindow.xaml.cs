using System;
using System.Globalization;
using System.Windows.Data;
using POESKillTree.Localization;

namespace POESKillTree.Views
{
    public class LastUpdatedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = value as DateTime?;
            if (dateTime == null)
                return null;
            var str = dateTime == DateTime.MinValue ? L10n.Message("Not Available") : dateTime.ToString();
            return string.Format(L10n.Message("Last updated: {0}"), str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Interaction logic for EditBuildWindow.xaml
    /// </summary>
    public partial class EditBuildWindow
    {

        public EditBuildWindow()
        {
            InitializeComponent();
        }
    }
}
