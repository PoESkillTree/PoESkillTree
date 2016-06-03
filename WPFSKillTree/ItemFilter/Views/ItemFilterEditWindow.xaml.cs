using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls;
using POESKillTree.ItemFilter.Model;
using POESKillTree.Utils;

namespace POESKillTree.ItemFilter.Views
{
    /// <summary>
    /// Interaction logic for ItemFilterEditWindow.xaml
    /// </summary>
    public partial class ItemFilterEditWindow : MetroWindow
    {
        public Filter Filter { get; set; }

        public ItemFilterEditWindow(Filter filter)
        {
            Filter = filter;

            InitializeComponent();

            DataContext = this;

            Title = Filter.Name + " - " + Title;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            string description = (sender as Button).Tag.ToString();

            Popup.Info(description);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
