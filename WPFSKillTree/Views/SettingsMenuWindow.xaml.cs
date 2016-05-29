using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Utils;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ColorComboBox_Initialized(object sender, System.EventArgs e)
        {
            var s = (ComboBox)sender;
            foreach (System.Reflection.PropertyInfo prop in typeof(Brushes).GetProperties())
            {
                var cbItem = new ComboBoxItem
                {
                    Content = prop.Name,
                    Foreground = (Brush) new BrushConverter().ConvertFromString(prop.Name)
                };
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
            var color = (ComboBoxItem)s.Items.CurrentItem;
            if(color != null)
                s.Foreground = (Brush)new BrushConverter().ConvertFromString(color.Content.ToString());
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

        private void ShowAllAscendancyClasses_Checked(object sender, RoutedEventArgs e)
        {
            _persistentData.Options.ShowAllAscendancyClasses = true;
            _persistentData.SavePersistentDataToFile();
        }

        private void ShowAllAscendancyClasses_Unchecked(object sender, RoutedEventArgs e)
        {
            _persistentData.Options.ShowAllAscendancyClasses = false;
            _persistentData.SavePersistentDataToFile();
        }

        private void ShowAllAscendancyClasses_Initialized(object sender, System.EventArgs e)
        {
            ((CheckBox)sender).IsChecked = _persistentData.Options.ShowAllAscendancyClasses;
        }

        private void Languages_Initialized(object sender, System.EventArgs e)
        {
            var s = ((ComboBox) sender);
            s.DisplayMemberPath = "Value";
            s.SelectedValuePath = "Key";
            s.ItemsSource = L10n.GetLanguages();
            s.SelectedValue = _persistentData.Options.Language;
        }

        private void Languages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = (KeyValuePair<string,string>) e.AddedItems[0];
            
            if (s.Key == _persistentData.Options.Language) return;

            Popup.Info(L10n.Message("You will need to restart the program for all changes to take effect."));

            _persistentData.Options.Language = s.Key;
            L10n.SetLanguage(s.Key);
            _persistentData.SavePersistentDataToFile();            
        }
    }
}
