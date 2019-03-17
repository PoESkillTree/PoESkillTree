using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.ItemFilter.Model;

namespace PoESkillTree.ItemFilter.Views
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

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string selected = FilterListBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selected))
            {
                await this.ShowErrorAsync("No filter selected!");

                return;
            }

            if (!FilterManager.IsValidFilterName(name))
            {
                await this.ShowErrorAsync(string.Format("'{0}' is incorrect item filter name.", name));

                return;
            }

            if (FilterNames.Contains(name))
            {
                await this.ShowErrorAsync(string.Format("Item filter '{0}' already exists.", name));

                return;
            }

            // Ask whether existing game filter should be overwritten.
            if (FilterManager.GameFilterExists(name))
            {
                string msg = string.Format("Item filter '{0}.filter' already exists in game folder.\nAre you sure you want to overwrite it?", name);
                if (await this.ShowQuestionAsync(msg) == MessageBoxResult.No)
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
                        string msg = saved ? "An error occurred while attempting to create game filter." : "An error occurred while attempting to save item filter.";
                        await this.ShowErrorAsync(msg, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("An error occurred while attempting to load item filter.", ex.Message);
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            string name = FilterListBox.SelectedItem as string;

            if (name == null) return; // No selection.

            if (await this.ShowQuestionAsync("Are you sure you want to delete this item filter?") == MessageBoxResult.No)
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

                await this.ShowErrorAsync("An error occurred while attempting to delete item filter.", ex.Message);
            }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
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
                        await this.ShowErrorAsync("An error occurred while attempting to save item filter.", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("An error occurred while attempting to load item filter.", ex.Message);
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

        private async void New_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;

            if (!FilterManager.IsValidFilterName(name))
            {
                await this.ShowErrorAsync(string.Format("'{0}' is incorrect item filter name.", name));

                return;
            }

            if (FilterNames.Contains(name))
            {
                await this.ShowErrorAsync(string.Format("Item filter '{0}' already exists.", name));

                return;
            }

            // Ask whether existing game filter should be overwritten.
            if (FilterManager.GameFilterExists(name))
            {
                string msg = string.Format("Item filter '{0}.filter' already exists in game folder.\nAre you sure you want to overwrite it?", name);
                if (await this.ShowQuestionAsync(msg) == MessageBoxResult.No)
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
                    string msg = saved ? "An error occurred while attempting to create game filter." : "An error occurred while attempting to save item filter.";
                    await this.ShowErrorAsync(msg, ex.Message);
                }
            }
        }
    }
}
