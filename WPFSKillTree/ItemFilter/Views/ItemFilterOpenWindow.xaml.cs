using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using POESKillTree.ItemFilter.Model;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for ItemFilterOpenWindow.xaml
    /// </summary>
    public partial class ItemFilterOpenWindow : MetroWindow
    {
        public ObservableCollection<string> FilterNames { get; set; }

        public ItemFilterOpenWindow()
        {
            InitializeComponent();

            DataContext = this;

            FilterNames = new ObservableCollection<string>(FilterManager.GetFilterNames());
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string selected = FilterListBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selected))
            {
                Popup.Error("No filter selected!");

                return;
            }

            if (!FilterManager.IsValidFilterName(name))
            {
                Popup.Error(string.Format("'{0}' is incorrect item filter name.", name));

                return;
            }

            if (FilterNames.Contains(name))
            {
                Popup.Error(string.Format("Item filter '{0}' already exists.", name));

                return;
            }

            // Ask whether existing game filter should be overwritten.
            if (FilterManager.GameFilterExists(name))
            {
                string msg = string.Format("Item filter '{0}.filter' already exists in game folder.\nAre you sure you want to overwrite it?", name);
                if (Popup.Ask(msg, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            try
            {
                // Load selected filter and rename it.
                Filter filter = FilterManager.Load(selected);
                filter.Name = name;

                var window = new ItemFilterEditWindow(filter) { Owner = this };
                if (window.ShowDialog() == true)
                {
                    bool saved = false;
                    try
                    {
                        FilterManager.Save(filter);

                        saved = true;
                        NameTextBox.Text = string.Empty;
                        FilterNames.Add(name);

                        FilterManager.Enable(filter);
                    }
                    catch (Exception ex)
                    {
                        if (saved)
                            Popup.Error("An error occurred while attempting to create game filter.", ex.Message);
                        else
                            Popup.Error("An error occurred while attempting to save item filter.", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Popup.Error("An error occurred while attempting to load item filter.", ex.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            string name = FilterListBox.SelectedItem as string;

            if (name == null) return; // No selection.

            if (Popup.Ask("Are you sure you want to delete this item filter?", MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            try
            {
                FilterManager.Delete(name);

                FilterNames.Remove(name);
            }
            catch (Exception ex)
            {
                // Check whether deletion of filter definition failed, or only game filter file.
                if (!FilterManager.Exists(name)) FilterNames.Remove(name);

                Popup.Error("An error occurred while attempting to delete item filter.", ex.Message);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (FilterListBox.SelectedItem == null) return; // No selection.

            try
            {
                Filter filter = FilterManager.Load(FilterListBox.SelectedItem as string);

                var window = new ItemFilterEditWindow(filter) { Owner = this };
                if (window.ShowDialog() == true)
                {
                    try
                    {
                        FilterManager.Save(filter);
                        FilterManager.Enable(filter);
                    } catch (Exception ex)
                    {
                        Popup.Error("An error occurred while attempting to save item filter.", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Popup.Error("An error occurred while attempting to load item filter.", ex.Message);
            }
        }

        private void FilterListBox_DoubleClick(object sender, RoutedEventArgs e)
        {
            // XXX: This requires that only elements with TextBlock inside of FilterListBox are ListBoxItem elements.
            if (e.OriginalSource is TextBlock)
            {
                Edit_Click(sender, new RoutedEventArgs(e.RoutedEvent, sender));
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;

            if (!FilterManager.IsValidFilterName(name))
            {
                Popup.Error(string.Format("'{0}' is incorrect item filter name.", name));

                return;
            }

            if (FilterNames.Contains(name))
            {
                Popup.Error(string.Format("Item filter '{0}' already exists.", name));

                return;
            }

            // Ask whether existing game filter should be overwritten.
            if (FilterManager.GameFilterExists(name))
            {
                string msg = string.Format("Item filter '{0}.filter' already exists in game folder.\nAre you sure you want to overwrite it?", name);
                if (Popup.Ask(msg, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            Filter filter = FilterManager.Create(name);

            var window = new ItemFilterEditWindow(filter) { Owner = this };
            if (window.ShowDialog() == true)
            {
                bool saved = false;
                try
                {
                    FilterManager.Save(filter);

                    saved = true;
                    NameTextBox.Text = string.Empty;
                    FilterNames.Add(name);

                    FilterManager.Enable(filter);
                }
                catch (Exception ex)
                {
                    if (saved)
                        Popup.Error("An error occurred while attempting to create game filter.", ex.Message);
                    else
                        Popup.Error("An error occurred while attempting to save item filter.", ex.Message);
                }
            }
        }
    }
}
