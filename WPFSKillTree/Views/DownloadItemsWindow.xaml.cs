using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using POESKillTree.Localization;

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

        public DownloadItemsWindow(string characterName, string accountName)
        {
            InitializeComponent();
            tbCharName.Text = string.IsNullOrEmpty(characterName) ? L10n.Message("CharacterName") : characterName;
            tbAccName.Text = string.IsNullOrEmpty(accountName) ? L10n.Message("AccountName") : accountName;
        }

        public string GetCharacterName()
        {
            return tbCharName.Text == L10n.Message("CharacterName") ? null : tbCharName.Text;
        }
        public string GetAccountName()
        {
            return tbAccName.Text == L10n.Message("AccountName") ? null : tbAccName.Text;
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
                tbCharLink.Text = "https://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text + "&accountName=" + tbAccName.Text;
                tbTreeLink.Text = "http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character=" + tbCharName.Text + "&accountName=" + tbAccName.Text;
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
                DialogResult = true;
            }
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbCharName.Focus();
            tbCharName.SelectAll();
        }
    }
}
