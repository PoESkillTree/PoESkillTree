using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using POESKillTree.Localization;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils;
using POESKillTree.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System.ComponentModel;
using System.Linq;
using POESKillTree.ViewModels.Items;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for DownloadItemsWindow.xaml
    /// </summary>
    public partial class DownloadStashWindow : MetroWindow, INotifyPropertyChanged
    {

        List<StashBookmark> _tabb = new List<StashBookmark>();

        public event PropertyChangedEventHandler PropertyChanged;

        public List<StashBookmark> Tabs
        {
            get
            {
                return _tabb;
            }

            set
            {
                _tabb = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Tabs"));
            }
        }

        public DownloadStashWindow()
        {
            InitializeComponent();
        }

        public DownloadStashWindow(string characterName, string accountName)
        {
            InitializeComponent();
            tbLeague.Text = string.IsNullOrEmpty(characterName) ? L10n.Message("League") : characterName;
            tbAccName.Text = string.IsNullOrEmpty(accountName) ? L10n.Message("AccountName") : accountName;
        }

        public string GetCharacterName()
        {
            return tbLeague.Text == L10n.Message("League") ? null : tbLeague.Text;
        }
        public string GetAccountName()
        {
            return tbAccName.Text == L10n.Message("AccountName") ? null : tbAccName.Text;
        }

        private void tbCharName_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbstashTabsLink.Text = "https://www.pathofexile.com/character-window/get-stash-items?tabs=1&tabIndex=0&league=" + tbLeague.Text + "&accountName=" + tbAccName.Text;
            cbTabs_SelectionChanged(null, null);
        }

        private void btnPopupOpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(tbstashTabsLink.Text);
        }

        private void btnPopupLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Multiselect = false };
            var ftoload = fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
                try
                {
                    var tabs = JObject.Parse(File.ReadAllText(fileDialog.FileName))["tabs"] as JArray;

                    List<StashBookmark> tabb = new List<StashBookmark>();

                    foreach (JObject tab in tabs)
                    {
                        string name = tab["n"].Value<string>();
                        int index = tab["i"].Value<int>();
                        var c = tab["colour"].Value<JObject>();
                        var color = Color.FromArgb(0xFF, c["r"].Value<byte>(), c["g"].Value<byte>(), c["b"].Value<byte>());
                        StashBookmark sb = new StashBookmark(name, index, color);
                        tabb.Add(sb);
                    }

                    Tabs = tabb;
                    cbTabs.SelectedIndex = 0;

                }
                catch (Exception ex)
                {
                    Popup.Error(L10n.Message("An error occurred while attempting to load stash data."), ex.Message);
                }
            }
        }


        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }



        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbAccName.Focus();
            tbAccName.SelectAll();
        }

        private void cbTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTabs.SelectedItem == null)
            {
                tbstashTabLink.Text = "";
            }
            else
            {
                var s = cbTabs.SelectedItem as StashBookmark;
                tbstashTabLink.Text = string.Format("https://www.pathofexile.com/character-window/get-stash-items?tabs=0&tabIndex={2}&league={0}&accountName={1}", tbLeague.Text, tbAccName.Text, s.Position);
            }
        }

        private void btnPopupOpenTabBrowser_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(tbstashTabLink.Text);
        }

        private void btnPopupLoadTabFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Multiselect = false };
            var ftoload = fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
                var cont = File.ReadAllText(fileDialog.FileName);
                var mw = this.Owner as MainWindow;
                if (mw == null)
                    return;

                var stash = mw.Stash;
                IntRange highlightrange = null;
                try
                {
                    var data = File.ReadAllText(fileDialog.FileName);

                    var json = JObject.Parse(data);
                    var items = (json["items"] as JArray).Select(i => new Item((JObject)i)).ToArray();

                    //get free line
                    var y = stash.LastOccupiedLine + 3;
                    var fittingitems = items.Where(i => i.X >= 0 && i.X + i.W <= 12).ToList();


                    StashBookmark sb = new StashBookmark("imported", y);
                    if (cbTabs.SelectedItem != null)
                    {
                        var sstab = cbTabs.SelectedItem as StashBookmark;
                        sb = new StashBookmark(sstab.Name, y, sstab.Color);
                    }


                    stash.BeginUpdate();
                    stash.AddBookmark(sb);
                    var my = fittingitems.Min(i => i.Y);
                    var y2 = y;
                    var unfittingitems = items.Where(i => i.X < 0 || i.X + i.W > 12);
                    foreach (var item in items)
                    {
                        item.Y += y - my;
                        stash.Items.Add(item);
                        if (y2 < item.Y + item.H)
                            y2 = item.Y + item.H;
                    }


                    int x = 0;
                    int maxh = 0;
                    var y3 = y2;
                    foreach (var item in unfittingitems)
                    {
                        if (x + item.W > 12) //next line
                        {
                            x = 0;
                            y2 += maxh;
                            maxh = 0;
                        }

                        item.X = x;
                        x += item.W;

                        if (maxh < item.H)
                            maxh = item.H;

                        item.Y = y2;
                        stash.Items.Add(item);

                        if (y3 < item.Y + item.H)
                            y3 = item.Y + item.H;
                    }
                    Popup.Info(string.Format(L10n.Message("New tab with {0} items was added to stash"),items.Length));
                    highlightrange = new IntRange() { From = y, Range = y3 - y };
                }
                catch (Exception ex)
                {
                    Popup.Error(L10n.Message("An error occurred while attempting to load stash data."), ex.Message);
                }
                finally
                {
                    stash.EndUpdate();
                }

                if (highlightrange != null)
                    stash.AddHighlightRange(highlightrange);
            }
        }
    }
}
