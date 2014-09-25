using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for DownloadItemsWindow.xaml
    /// </summary>
    public partial class DownloadItemsWindow : MetroWindow
    {
        public DownloadItemsWindow()
        {
            InitializeComponent();
        }

        public DownloadItemsWindow(string characterName, bool showClear)
        {
            InitializeComponent();
            tbCharName.Text = string.IsNullOrEmpty(characterName) ? "YourCharacterName" : characterName;
            if (showClear)
                btnPopupClear.Visibility = Visibility.Visible;
        }

        public string GetCharacterName()
        {
            return tbCharName.Text;
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
                tbCharLink.Text = "https://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
                tbTreeLink.Text = "http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character=" + tbCharName.Text;
        }

        private void btnPopupOpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(tbCharLink.Text);
        }

        private void btnPopupLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Multiselect = false };
            var ftoload = fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
                var itemData = File.ReadAllText(fileDialog.FileName);
                (Owner as MainWindow).LoadItemData(itemData);
                btnPopupClear.Visibility = Visibility.Visible;
            }
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnPopupClear_Click(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow).ClearCurrentItemData();
            btnPopupClear.Visibility = Visibility.Collapsed;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbCharName.Focus();
            tbCharName.SelectAll();
        }
    }
}
