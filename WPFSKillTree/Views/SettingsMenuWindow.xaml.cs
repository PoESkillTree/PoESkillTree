using System.Windows;
using MahApps.Metro.Controls;
using POESKillTree.Localization;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using POESKillTree.Utils;
using System.Configuration;
using POESKillTree.Model;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for SettingsMenuWindow.xaml
    /// </summary>
    public partial class SettingsMenuWindow : MetroWindow
    {
        private readonly PersistentData _persistentData = App.PersistentData;

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

                switch (s.Name)
                {
                    case "NodeHoverHighlightColor":
                        if (_persistentData.Options.NodeHoverHighlightColor == cbItem.Content.ToString())
                        {
                            s.SelectedIndex = s.Items.IndexOf(cbItem);
                            s.Foreground = (Brush)new BrushConverter().ConvertFromString(cbItem.Content.ToString());
                        }
                        break;
                    case "NodeAttrHighlightColor":
                        if (_persistentData.Options.NodeAttrHighlightColor == cbItem.Content.ToString())
                        {
                            s.SelectedIndex = s.Items.IndexOf(cbItem);
                            s.Foreground = (Brush)new BrushConverter().ConvertFromString(cbItem.Content.ToString());
                        }
                        break;
                    case "NodeSearchHighlightColor":
                        if (_persistentData.Options.NodeSearchHighlightColor == cbItem.Content.ToString())
                        {
                            s.SelectedIndex = s.Items.IndexOf(cbItem);
                            s.Foreground = (Brush)new BrushConverter().ConvertFromString(cbItem.Content.ToString());
                        }
                        break;
                    default:
                        break;
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
            switch(name) 
            {
                case "NodeHoverHighlightColor":
                    _persistentData.Options.NodeHoverHighlightColor = value;
                    break;
                case "NodeAttrHighlightColor":
                    _persistentData.Options.NodeAttrHighlightColor = value;
                    break;
                case "NodeSearchHighlightColor":
                    _persistentData.Options.NodeSearchHighlightColor = value;
                    break;
                default:
                    break;
            }
            _persistentData.SavePersistentDataToFile();
        }
    }
}
