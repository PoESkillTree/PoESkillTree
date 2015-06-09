using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Linq;
using POESKillTree.ViewModels.Items;

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

        public DownloadItemsWindow(string characterName)
        {
            InitializeComponent();
            tbCharName.Text = string.IsNullOrEmpty(characterName) ? "YourCharacterName" : characterName;
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
                var mw = (Owner as MainWindow);
                mw.LoadItemData(itemData);

                var items = ((JArray)JObject.Parse(itemData).Property("items").Value);
                mw.Stash.AddItems(items.Select(i => new Item((JObject)i)), "EquipImport");


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
