using System.Windows;
using MahApps.Metro.Controls;
using POESKillTree.Localization;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using POESKillTree.Utils;
using System.Configuration;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for SettingsMenuWindow.xaml
    /// </summary>
    public partial class SettingsMenuWindow : MetroWindow
    {
        public SettingsMenuWindow()
        {
            InitializeComponent();
        }

        private void ColorComboBox_Initialized(object sender, System.EventArgs e)
        {
            var s = (ComboBox)sender;
            foreach (System.Reflection.PropertyInfo prop in typeof(Brushes).GetProperties())
            {
                ComboBoxItem cbItem = new ComboBoxItem();
                cbItem.Content = prop.Name;
                cbItem.Foreground = (Brush)new BrushConverter().ConvertFromString(prop.Name);
                s.Items.Add(cbItem);
                var property = Properties.Settings.Default[s.Name];
                var type = property.GetType();
                if(type == typeof(System.Drawing.Color))
                {
                    if (((System.Drawing.Color)property).Name.ToString() == cbItem.Content.ToString())
                    {
                        s.SelectedIndex = s.Items.IndexOf(cbItem);
                        s.Foreground = (Brush)new BrushConverter().ConvertFromString(cbItem.Content.ToString());
                    }
                }
            }
        }

        private void ColorComboBox_Closed(object sender, System.EventArgs e)
        {
            var s = (ComboBox)sender;
            ComboBoxItem color = (ComboBoxItem)s.Items.CurrentItem;
            if(color != null)
                s.Foreground = (Brush)new BrushConverter().ConvertFromString(color.Content.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = (ComboBox)sender;
            var value = ((ComboBoxItem)s.Items.CurrentItem).Content.ToString();
            SaveSetting(s.Name, value);
        }
        private void SaveSetting(string name, string value)
        {
            Properties.Settings.Default[name] = System.Drawing.Color.FromName(value);
            Properties.Settings.Default.Save();
        }
    }
}
