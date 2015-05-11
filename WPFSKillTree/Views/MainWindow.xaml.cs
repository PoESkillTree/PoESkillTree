using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using POESKillTree.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.ViewModels;
using POESKillTree.ViewModels.ItemAttribute;
using Application = System.Windows.Application;
using Attribute = POESKillTree.ViewModels.Attribute;
using Clipboard = System.Windows.Clipboard;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = POESKillTree.Views.MetroMessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using ToolTip = System.Windows.Controls.ToolTip;

namespace POESKillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly PersistentData _persistentData = App.PersistentData;

        public PersistentData PersistentData
        {
            get { return _persistentData; }
        }


        private static readonly Action EmptyDelegate = delegate { };
        private readonly List<Attribute> _allAttributesList = new List<Attribute>();
        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly List<ListGroupItem> _defenceList = new List<ListGroupItem>();
        private readonly Dictionary<string, AttributeGroup> _defenceListGroups = new Dictionary<string, AttributeGroup>();
        private readonly List<ListGroupItem> _offenceList = new List<ListGroupItem>();
        private readonly Dictionary<string, AttributeGroup> _offenceListGroups = new Dictionary<string, AttributeGroup>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
        private readonly ToolTip _noteTip = new ToolTip();
        private ListCollectionView _allAttributeCollection;
        private ListCollectionView _attibuteCollection;
        private ListCollectionView _defenceCollection;
        private ListCollectionView _offenceCollection;
        private RenderTargetBitmap _clipboardBmp;

        private ItemAttributes _itemAttributes;
        protected SkillTree Tree;
        private const string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private Vector2D _addtransform;
        private bool _justLoaded;
        private string _lasttooltip;

        private LoadingWindow _loadingWindow;
        private Vector2D _multransform;

        private List<ushort> _prePath;
        private HashSet<ushort> _toRemove;

        readonly Stack<string> _undoList = new Stack<string>();
        readonly Stack<string> _redoList = new Stack<string>();

        private Point _dragAndDropStartPoint;
        private DragAdorner _adorner;
        private AdornerLayer _layer;

        private MouseButton _lastMouseButton;
        private String _highlightedAttribute = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ItemDB.Load("Items.xml");
            if (File.Exists("ItemsLocal.xml"))
                ItemDB.Merge("ItemsLocal.xml");
            ItemDB.Index();

            _attibuteCollection = new ListCollectionView(_attiblist);
            listBox1.ItemsSource = _attibuteCollection;
            _attibuteCollection.GroupDescriptions.Add(new PropertyGroupDescription("Text")
            {
                Converter = new GroupStringConverter()
            });

            _allAttributeCollection = new ListCollectionView(_allAttributesList);
            _allAttributeCollection.GroupDescriptions.Add(new PropertyGroupDescription("Text")
            {
                Converter = new GroupStringConverter()
            });
            lbAllAttr.ItemsSource = _allAttributeCollection;

            _defenceCollection = new ListCollectionView(_defenceList);
            _defenceCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            listBoxDefence.ItemsSource = _defenceCollection;

            _offenceCollection = new ListCollectionView(_offenceList);
            _offenceCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            listBoxOffence.ItemsSource = _offenceCollection;

            // Set theme & accent.
            SetTheme(_persistentData.Options.Theme);
            SetAccent(_persistentData.Options.Accent);

            Tree = SkillTree.CreateSkillTree(StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
            Tree.MainWindow = this;
            recSkillTree.Width = SkillTree.TRect.Width / SkillTree.TRect.Height * 500;
            recSkillTree.UpdateLayout();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

            Tree.Chartype =
                SkillTree.CharName.IndexOf(((string)((ComboBoxItem)cbCharType.SelectedItem).Content).ToUpper());
            Tree.UpdateAvailNodes();
            UpdateUI();

            _multransform = SkillTree.TRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = SkillTree.TRect.TopLeft;

            // loading last build
            if (_persistentData.CurrentBuild != null)
                SetCurrentBuild(_persistentData.CurrentBuild);

            btnLoadBuild_Click(this, new RoutedEventArgs());
            _justLoaded = false;
            // loading saved build
            lvSavedBuilds.Items.Clear();
            foreach (var build in _persistentData.Builds)
            {
                lvSavedBuilds.Items.Add(build);
            }

            ImportLegacySavedBuilds();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ToggleAttributes();
                        break;
                    case Key.B:
                        ToggleBuilds();
                        break;
                    case Key.R:
                        btnReset_Click(sender, e);
                        break;
                    case Key.E:
                        btnPoeUrl_Click(sender, e);
                        break;
                    case Key.D1:
                        cbCharType.SelectedIndex = 0;
                        break;
                    case Key.D2:
                        cbCharType.SelectedIndex = 1;
                        break;
                    case Key.D3:
                        cbCharType.SelectedIndex = 2;
                        break;
                    case Key.D4:
                        cbCharType.SelectedIndex = 3;
                        break;
                    case Key.D5:
                        cbCharType.SelectedIndex = 4;
                        break;
                    case Key.D6:
                        cbCharType.SelectedIndex = 5;
                        break;
                    case Key.D7:
                        cbCharType.SelectedIndex = 6;
                        break;
                    case Key.Z:
                        tbSkillURL_Undo();
                        break;
                    case Key.Y:
                        tbSkillURL_Redo();
                        break;
                    case Key.S:
                        SaveNewBuild();
                        break;
                }
            }

            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ToggleCharacterSheet();
                        break;
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _persistentData.CurrentBuild.Url = tbSkillURL.Text;
            _persistentData.CurrentBuild.Level = tbLevel.Text;
            _persistentData.SavePersistentDataToFile();

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
        }

        #endregion

        #region LoadingWindow

        private void StartLoadingWindow()
        {
            _loadingWindow = new LoadingWindow();
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            _loadingWindow.Show();
        }

        private void UpdateLoadingWindow(double c, double max)
        {
            _loadingWindow.progressBar1.Maximum = max;
            _loadingWindow.progressBar1.Value = c;
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            if (Equals(c, max))
                Thread.Sleep(100);
        }

        private void CloseLoadingWindow()
        {
            _loadingWindow.Close();
        }

        #endregion

        #region Menu

        private void Menu_SkillHighlightedNodes(object sender, RoutedEventArgs e)
        {
            var currentCursor = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                Tree.SkillAllHighlightedNodes();
                UpdateUI();
                tbSkillURL.Text = Tree.SaveToURL();
            }
            finally
            {
                Cursor = currentCursor;
            }
        }

        private void Menu_UnhighlightAllNodes(object sender, RoutedEventArgs e)
        {
            Tree.UnhighlightAllNodes();
            ClearSearch();
        }

        private void Menu_ScreenShot(object sender, RoutedEventArgs e)
        {
            const int maxsize = 3000;
            Rect2D contentBounds = Tree.picActiveLinks.ContentBounds;
            contentBounds *= 1.2;
            if (!double.IsNaN(contentBounds.Width) && !double.IsNaN(contentBounds.Height))
            {
                double aspect = contentBounds.Width / contentBounds.Height;
                double xmax = contentBounds.Width;
                double ymax = contentBounds.Height;
                if (aspect > 1 && xmax > maxsize)
                {
                    xmax = maxsize;
                    ymax = xmax / aspect;
                }
                if (aspect < 1 & ymax > maxsize)
                {
                    ymax = maxsize;
                    xmax = ymax * aspect;
                }

                _clipboardBmp = new RenderTargetBitmap((int)xmax, (int)ymax, 96, 96, PixelFormats.Pbgra32);
                var db = new VisualBrush(Tree.SkillTreeVisual);
                db.ViewboxUnits = BrushMappingMode.Absolute;
                db.Viewbox = contentBounds;
                var dw = new DrawingVisual();

                using (DrawingContext dc = dw.RenderOpen())
                {
                    dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
                }
                _clipboardBmp.Render(dw);
                _clipboardBmp.Freeze();

                Clipboard.SetImage(_clipboardBmp);

                recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
            }
        }

        private void Menu_ImportItems(object sender, RoutedEventArgs e)
        {
            var diw = new DownloadItemsWindow(_persistentData.CurrentBuild.CharacterName, _persistentData.CurrentBuild.AccountName) { Owner = this };
            diw.ShowDialog();
            _persistentData.CurrentBuild.CharacterName = diw.GetCharacterName();
            _persistentData.CurrentBuild.AccountName = diw.GetAccountName();
        }

        private void Menu_ClearItems(object sender, RoutedEventArgs e)
        {
            ClearCurrentItemData();
        }

        private void Menu_CopyStats(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var at in _attiblist)
            {
                sb.AppendLine(at.ToString());
            }
            try
            {
                System.Windows.Forms.Clipboard.SetText(sb.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Clipboard could not be copied to. Please try again.", "Failed Copy!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Menu_RedownloadTreeAssets(object sender, RoutedEventArgs e)
        {
            const string sMessageBoxText = "This will delete your data folder and Redownload all the SkillTree assets.\nThis requires an internet connection!\n\nDo you want to proced?";
            const string sCaption = "Redownload SkillTree Assets - Warning";

            var rsltMessageBox = MessageBox.Show(this, sMessageBoxText, sCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    if (Directory.Exists("Data"))
                    {
                        try
                        {
                            if (Directory.Exists("DataBackup"))
                                Directory.Delete("DataBackup", true);
                            Directory.Move("Data", "DataBackup");

                            Tree = SkillTree.CreateSkillTree(StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
                            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);


                            SkillTree.ClearAssets();//enable recaching of assets
                            SkillTree.CreateSkillTree();//create new skilltree to reinitialize cache


                            btnLoadBuild_Click(this, new RoutedEventArgs());
                            _justLoaded = false;

                            if (Directory.Exists("DataBackup"))
                                Directory.Delete("DataBackup", true);
                        }
                        catch (Exception ex)
                        {
                            if (Directory.Exists("Data"))
                                Directory.Delete("Data", true);
                            try
                            {
                                CloseLoadingWindow();
                            }
                            catch (Exception)
                            {
                                //Nothing
                            }
                            Directory.Move("DataBackup", "Data");
                            MessageBox.Show(this, ex.Message.ToString(), "Error while downloading assets", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;

                case MessageBoxResult.No:
                    //Do nothing
                    break;
            }
        }

        private void Menu_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Menu_OpenPoEWebsite(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.pathofexile.com/");
        }

        private void Menu_OpenWiki(object sender, RoutedEventArgs e)
        {
            Process.Start("http://pathofexile.gamepedia.com/");
        }

        private void Menu_OpenHelp(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow() { Owner = this };
            helpWindow.ShowDialog();
        }

        private void Menu_OpenHotkeys(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new HotkeysWindow() { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void Menu_OpenAbout(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow() { Owner = this };
            aboutWindow.ShowDialog();
        }

        // Checks for updates.
        private void Menu_CheckForUpdates(object sender, RoutedEventArgs e)
        {
            try
            {
                Updater.Release release = Updater.CheckForUpdates();
                if (release == null)
                {
                    MessageBox.Show(this, L10n.Message("You have the lastest version!"), L10n.Message("No update"));
                }
                else
                {
                    var message = String.Format(L10n.Message("Do you want to install version {0}?"), release.Version);
                    MessageBoxResult download = new MessageBoxResult();
                    if (release.Version.ToLower().Contains("pre"))
                    {
                        download = MessageBox.Show(this, String.Format(L10n.Message("{0}\nThis is a pre-release, meaning there could be some bugs!"), message), L10n.Message("New pre-release"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    else
                        download = MessageBox.Show(this, message, L10n.Message("New release"), MessageBoxButton.YesNo, MessageBoxImage.None);

                    if (download == MessageBoxResult.Yes)
                        btnUpdateInstall(sender, e);
                    else
                        btnUpdateCancel(sender, e);
                    // Show dialog with release informations and "Install & Restart" button.
                }
            }
            catch (UpdaterException ex)
            {
                // Display error message: ex.Message.
                MessageBox.Show(this, ex.Message.ToString(), L10n.Message("Error occured"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Starts update process.
        private void btnUpdateInstall(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show download progress bar and Cancel button.
                // Start downloading.
                StartLoadingWindow();
                Updater.Download(UpdateDownloadCompleted, UpdateDownloadProgressChanged);
            }
            catch (UpdaterException ex)
            {
                // Display error message: ex.Message.
                MessageBox.Show(this, ex.Message.ToString(), L10n.Message("Update failed"));
            }
        }

        // Cancels update download (also invoked when download progress dialog is closed).
        private void btnUpdateCancel(object sender, RoutedEventArgs e)
        {
            if (Updater.IsDownloading)
                Updater.Cancel();
            else
            {
                Updater.Dispose();
                // Close dialog.
            }
        }

        // Invoked when update download completes, aborts or fails.
        private void UpdateDownloadCompleted(Object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) // Check whether download was cancelled.
            {
                Updater.Dispose();
                // Close dialog.
            }
            else if (e.Error != null) // Check whether error occured.
            {
                // Display error message: e.Error.Message.
                MessageBox.Show(this, e.Error.Message.ToString(), L10n.Message("Update failed"));
            }
            else // Download completed.
            {
                try
                {
                    Updater.Install();
                    Updater.RestartApplication();
                }
                catch (UpdaterException ex)
                {
                    Updater.Dispose();
                    // Display error message: ex.Message.
                    MessageBox.Show(this, ex.Message.ToString(), L10n.Message("Update failed"));
                }
            }
            CloseLoadingWindow();
        }

        // Invoked when update download progress changes.
        private void UpdateDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update download progres bar.
            UpdateLoadingWindow(e.BytesReceived, e.TotalBytesToReceive);
        }

        #endregion

        #region  Character Selection

        private void cbCharType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_justLoaded)
            {
                _justLoaded = false;
                return;
            }

            if (Tree == null)
                return;
            var ComboItem = (ComboBoxItem)cbCharType.SelectedItem;
            var className = ComboItem.Name;

            if (Tree.CanSwitchClass(className))
            {
                var currentClassArray = getCurrentClass();
                var changeClassArray = getAnyClass(className);

                if (currentClassArray[0] == "ERROR")
                    return;
                if (changeClassArray[0] == "ERROR")
                    return;
                var usedPoints = tbUsedPoints.Text;
                cbCharType.Text = changeClassArray[0];

                Tree.LoadFromURL(tbSkillURL.Text.Replace(currentClassArray[1], changeClassArray[1]));
                tbUsedPoints.Text = usedPoints;
            }
            else
            {
                var startnode =
                    SkillTree.Skillnodes.First(
                        nd => nd.Value.Name.ToUpper() == (SkillTree.CharName[cbCharType.SelectedIndex]).ToUpper()).Value;
                Tree.SkilledNodes.Clear();
                Tree.SkilledNodes.Add(startnode.Id);
                Tree.Chartype = SkillTree.CharName.IndexOf((SkillTree.CharName[cbCharType.SelectedIndex]).ToUpper());
            }
            Tree.UpdateAvailNodes();
            UpdateUI();
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void tbLevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lvl;
            if (!int.TryParse(tbLevel.Text, out lvl)) return;
            Tree.Level = lvl;
            UpdateUI();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (Tree == null)
                return;
            Tree.Reset();
            UpdateUI();
            tbSkillURL.Text = Tree.SaveToURL();
        }

        #endregion

        #region Update Attribute and Character lists

        public void UpdateUI()
        {
            UpdateAttributeList();
            UpdateStatistics();
            UpdateClass();
        }

        public void UpdateAllAttributeList()
        {
            _allAttributesList.Clear();

            if (_itemAttributes != null)
            {
                Dictionary<string, List<float>> attritemp = Tree.SelectedAttributesWithoutImplicit;
                foreach (ItemAttributes.Attribute mod in _itemAttributes.NonLocalMods)
                {
                    if (attritemp.ContainsKey(mod.TextAttribute))
                    {
                        for (int i = 0; i < mod.Value.Count; i++)
                        {
                            attritemp[mod.TextAttribute][i] += mod.Value[i];
                        }
                    }
                    else
                    {
                        attritemp[mod.TextAttribute] = mod.Value;
                    }
                }

                foreach (var a in SkillTree.ImplicitAttributes(attritemp, Tree.Level))
                {
                    string key = SkillTree.RenameImplicitAttributes.ContainsKey(a.Key)
                        ? SkillTree.RenameImplicitAttributes[a.Key]
                        : a.Key;

                    if (!attritemp.ContainsKey(key))
                        attritemp[key] = new List<float>();
                    for (int i = 0; i < a.Value.Count; i++)
                    {
                        if (attritemp.ContainsKey(key) && attritemp[key].Count > i)
                            attritemp[key][i] += a.Value[i];
                        else
                        {
                            attritemp[key].Add(a.Value[i]);
                        }
                    }
                }

                foreach (string item in (attritemp.Select(InsertNumbersInAttributes)))
                {
                    var a = new Attribute(item);
                    _allAttributesList.Add(a);
                }
            }

            _allAttributeCollection.Refresh();
        }
        public void UpdateClass()
        {
            cbCharType.SelectedIndex = Tree.Chartype;
        }
        public void UpdateAttributeList()
        {
            _attiblist.Clear();
            Dictionary<string, List<float>> copy = (Tree.HighlightedAttributes == null) ? null : new Dictionary<string, List<float>>(Tree.HighlightedAttributes);

            foreach (var item in Tree.SelectedAttributes)
            {
                var a = new Attribute(InsertNumbersInAttributes(item));
                if (copy != null && copy.ContainsKey(item.Key))
                {
                    var citem = copy[item.Key];
                    a.Deltas = item.Value.Zip(citem, (s, h) => s - h).ToArray();
                    copy.Remove(item.Key);
                }
                else
                {
                    a.Deltas = (copy != null) ? item.Value.ToArray() : item.Value.Select(v => 0f).ToArray();
                }
                _attiblist.Add(a);
            }

            if (copy != null)
            {
                foreach (var item in copy)
                {
                    var a = new Attribute(InsertNumbersInAttributes(new KeyValuePair<string, List<float>>(item.Key, item.Value.Select(v => 0f).ToList())));
                    a.Deltas = item.Value.Select((h) => 0 - h).ToArray();
                    // if(item.Value.Count == 0)
                    a.Missing = true;
                    _attiblist.Add(a);
                }
            }

            _attibuteCollection.Refresh();
            tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
        }

        public void UpdateStatistics()
        {
            _defenceList.Clear();
            _offenceList.Clear();

            if (_itemAttributes != null)
            {
                Compute.Initialize(Tree, _itemAttributes);

                foreach (ListGroup group in Compute.Defense())
                {
                    foreach (var item in group.Properties.Select(InsertNumbersInAttributes))
                    {
                        AttributeGroup attributeGroup;
                        if (!_defenceListGroups.TryGetValue(group.Name, out attributeGroup))
                        {
                            attributeGroup = new AttributeGroup(group.Name);
                            _defenceListGroups.Add(group.Name, attributeGroup);
                        }
                        _defenceList.Add(new ListGroupItem(item, attributeGroup));
                    }
                }

                foreach (ListGroup group in Compute.Offense())
                {
                    foreach (var item in group.Properties.Select(InsertNumbersInAttributes))
                    {
                        AttributeGroup attributeGroup;
                        if (!_offenceListGroups.TryGetValue(group.Name, out attributeGroup))
                        {
                            attributeGroup = new AttributeGroup(group.Name);
                            _offenceListGroups.Add(group.Name, attributeGroup);
                        }
                        _offenceList.Add(new ListGroupItem(item, attributeGroup));
                    }
                }
            }

            _defenceCollection.Refresh();
            _offenceCollection.Refresh();
        }

        private string InsertNumbersInAttributes(KeyValuePair<string, List<float>> attrib)
        {
            string s = attrib.Key;
            foreach (float f in attrib.Value)
            {
                s = _backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        #endregion

        #region Attribute and Character lists - Event Handlers

        private void ToggleAttributes()
        {
            _persistentData.Options.AttributesBarOpened = !_persistentData.Options.AttributesBarOpened;
        }

        private void ToggleAttributes(bool expanded)
        {
            _persistentData.Options.AttributesBarOpened = expanded;
        }

        private void ToggleCharacterSheet()
        {
            _persistentData.Options.CharacterSheetBarOpened = !_persistentData.Options.CharacterSheetBarOpened;
        }

        private void ToggleCharacterSheet(bool expanded)
        {
            _persistentData.Options.CharacterSheetBarOpened = expanded;
        }

        private void expAttributes_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group collapsion events.
            {
                ToggleCharacterSheet(false);
            }
        }

        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var newHighlightedAttribute =
                Regex.Replace(
                    Regex.Match(listBox1.SelectedItem.ToString(), @"(?!\d)\w.*\w")
                        .Value.Replace(@"+", @"\+")
                        .Replace(@"-", @"\-")
                        .Replace(@"%", @"\%"), @"\d+", @"\d+");
            _highlightedAttribute = newHighlightedAttribute == _highlightedAttribute ? "" : newHighlightedAttribute;
            Tree.HighlightNodesBySearch(_highlightedAttribute, true, false);
        }

        private void expAttributes_MouseLeave(object sender, MouseEventArgs e)
        {
            SearchUpdate();
        }

        private void expCharacterSheet_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group expansion events.
            {
                ToggleAttributes(false);
            }
        }

        private void ToggleBuilds()
        {
            _persistentData.Options.BuildsBarOpened = !_persistentData.Options.BuildsBarOpened;
        }

        #endregion

        #region zbSkillTreeBackground

        private void zbSkillTreeBackground_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _lastMouseButton = e.ChangedButton;
        }

        private void zbSkillTreeBackground_Click(object sender, RoutedEventArgs e)
        {
            Point p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
            Size size = zbSkillTreeBackground.Child.DesiredSize;
            var v = new Vector2D(p.X, p.Y);

            v = v * _multransform + _addtransform;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
            {
                var node = nodes.First().Value;

                // Ignore clicks on character portraits and masteries
                if (node.Spc == null && !node.IsMastery)
                {
                    if (_lastMouseButton == MouseButton.Right)
                    {
                        Tree.ToggleNodeHighlight(node);
                        e.Handled = true;
                    }
                    else
                    {
                        // Toggle whether the node is included in the tree
                        if (Tree.SkilledNodes.Contains(node.Id))
                        {
                            Tree.ForceRefundNode(node.Id);
                            UpdateUI();

                            _prePath = Tree.GetShortestPathTo(node.Id, Tree.SkilledNodes);
                            Tree.DrawPath(_prePath);
                        }
                        else if (_prePath != null)
                        {
                            foreach (ushort i in _prePath)
                            {
                                Tree.SkilledNodes.Add(i);
                            }
                            UpdateUI();
                            Tree.UpdateAvailNodes();

                            _toRemove = Tree.ForceRefundNodePreview(node.Id);
                            if (_toRemove != null)
                                Tree.DrawRefundPreview(_toRemove);
                        }
                    }
                }
            }
            else
            {
                if (p.X < 0 || p.Y < 0 || p.X > size.Width || p.Y > size.Height)
                {
                    if (_lastMouseButton == MouseButton.Right)
                    {
                        zbSkillTreeBackground.Reset();
                    }
                }
            }
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void zbSkillTreeBackground_MouseLeave(object sender, MouseEventArgs e)
        {
            // We might have popped up a tooltip while the window didn't have focus,
            // so we should close tooltips whenever the mouse leaves the canvas in addition to
            // whenever we lose focus.
            _sToolTip.IsOpen = false;
        }

        private void zbSkillTreeBackground_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;
            textBox1.Text = "" + v.X;
            textBox2.Text = "" + v.Y;
            SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
                node = nodes.First().Value;

            if (node != null && node.Attributes.Count != 0)
            {
                if (Tree.SkilledNodes.Contains(node.Id))
                {
                    _toRemove = Tree.ForceRefundNodePreview(node.Id);
                    if (_toRemove != null)
                        Tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = Tree.GetShortestPathTo(node.Id, Tree.SkilledNodes);
                    Tree.DrawPath(_prePath);
                }

                var tooltip = node.Name + "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(_sToolTip.IsOpen && _lasttooltip == tooltip))
                {
                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock
                    {
                        Text = tooltip
                    });
                    if (_prePath != null)
                    {
                        sp.Children.Add(new Separator());
                        sp.Children.Add(new TextBlock { Text = "Points to skill node: " + _prePath.Count });
                    }

                    _sToolTip.Content = sp;
                    _sToolTip.IsOpen = true;
                    _lasttooltip = tooltip;
                }
            }
            else
            {
                _sToolTip.Tag = false;
                _sToolTip.IsOpen = false;
                _prePath = null;
                _toRemove = null;
                if (Tree != null)
                {
                    Tree.ClearPath();
                }
            }
        }

        private void zbSkillTreeBackground_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //zbSkillTreeBackground.Child.RaiseEvent(e);
        }

        #endregion

        #region Items

        public void LoadItemData(string itemData)
        {
            if (!string.IsNullOrEmpty(itemData))
            {
                try
                {
                    _persistentData.CurrentBuild.ItemData = itemData;
                    _itemAttributes = new ItemAttributes(itemData);
                    lbItemAttr.ItemsSource = _itemAttributes.Attributes;
                    mnuClearItems.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Item data currupted!");
                    _persistentData.CurrentBuild.ItemData = "";
                    _itemAttributes = null;
                    lbItemAttr.ItemsSource = null;
                    ClearCurrentItemData();
                }
            }
            else
            {
                ClearCurrentItemData();
            }

            UpdateUI();
        }

        public void ClearCurrentItemData()
        {
            _persistentData.CurrentBuild.ItemData = "";
            _itemAttributes = null;
            lbItemAttr.ItemsSource = null;
            UpdateUI();
            mnuClearItems.IsEnabled = false;
        }

        #endregion

        #region Builds - Event Handlers

        private void SavedBuildFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavedBuildFilterChanged();
        }

        private void SavedBuildFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            SavedBuildFilterChanged();
        }

        private void SavedBuildFilterChanged()
        {
            if (lvSavedBuilds == null) return;

            var selectedItem = (ComboBoxItem)cbCharTypeSavedBuildFilter.SelectedItem;
            var className = selectedItem.Content.ToString();
            var filterText = tbSavedBuildFilter.Text.ToLower();

            foreach (PoEBuild item in lvSavedBuilds.Items)
            {
                item.Visible = (className.Equals("All") || item.Class.Equals(className)) &&
                    (item.Name.ToLower().Contains(filterText) || item.Note.ToLower().Contains(filterText));
            }

            lvSavedBuilds.Items.Refresh();
        }

        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lvi = ((ListView)sender).SelectedItem;
            if (lvi == null) return;
            var build = ((PoEBuild)lvi);
            SetCurrentBuild(build);
            btnLoadBuild_Click(this, null); // loading the build
        }

        private void lvi_MouseLeave(object sender, MouseEventArgs e)
        {
            _noteTip.IsOpen = false;
        }

        private void lvi_MouseEnter(object sender, MouseEventArgs e)
        {
            var highlightedItem = FindListViewItem(e);
            if (highlightedItem != null)
            {
                var build = (PoEBuild)highlightedItem.Content;
                _noteTip.Content = build.Note == @"" ? L10n.Message("Right click to edit") : build.Note;
                _noteTip.IsOpen = true;
            }
        }

        private void lvi_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedBuild = (PoEBuild)lvSavedBuilds.SelectedItem;
            var formBuildName = new FormChooseBuildName(selectedBuild);
            formBuildName.Owner = this;
            var show_dialog = formBuildName.ShowDialog();
            if (show_dialog != null && (bool)show_dialog)
            {
                selectedBuild.Name = formBuildName.GetBuildName();
                selectedBuild.Note = formBuildName.GetNote();
                selectedBuild.CharacterName = formBuildName.GetCharacterName();
                selectedBuild.AccountName = formBuildName.GetAccountName();
                selectedBuild.ItemData = formBuildName.GetItemData();
                lvSavedBuilds.Items.Refresh();
            }
            SaveBuildsToFile();
        }

        private ListViewItem FindListViewItem(MouseEventArgs e)
        {
            var visualHitTest = VisualTreeHelper.HitTest(lvSavedBuilds, e.GetPosition(lvSavedBuilds)).VisualHit;

            ListViewItem listViewItem = null;

            while (visualHitTest != null)
            {
                if (visualHitTest is ListViewItem)
                {
                    listViewItem = visualHitTest as ListViewItem;

                    break;
                }
                if (Equals(visualHitTest, lvSavedBuilds))
                {
                    return null;
                }

                visualHitTest = VisualTreeHelper.GetParent(visualHitTest);
            }

            return listViewItem;
        }

        private void btnSaveNewBuild_Click(object sender, RoutedEventArgs e)
        {
            SaveNewBuild();
        }

        private void btnOverwriteBuild_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                var selectedBuild = (PoEBuild)lvSavedBuilds.SelectedItem;
                selectedBuild.Class = cbCharType.Text;
                selectedBuild.CharacterName = _persistentData.CurrentBuild.CharacterName;
                selectedBuild.AccountName = _persistentData.CurrentBuild.AccountName;
                selectedBuild.Level = tbLevel.Text;
                selectedBuild.PointsUsed = tbUsedPoints.Text;
                selectedBuild.Url = tbSkillURL.Text;
                selectedBuild.ItemData = _persistentData.CurrentBuild.ItemData;
                selectedBuild.LastUpdated = DateTime.Now;
                lvSavedBuilds.Items.Refresh();
                SaveBuildsToFile();
            }
            else
            {
                MessageBox.Show(this, L10n.Message("Please select a saved build!"), L10n.Message("Error"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count > 0)
            {
                lvSavedBuilds.Items.Remove(lvSavedBuilds.SelectedItem);
                SaveBuildsToFile();
            }
        }

        private void lvSavedBuilds_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                lvSavedBuilds.SelectedIndex > 0)
            {
                MoveBuildInList(-1);
            }

            else if (e.Key == Key.Down && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                     lvSavedBuilds.SelectedIndex < lvSavedBuilds.Items.Count - 1)
            {
                MoveBuildInList(1);
            }
        }

        private void MoveBuildInList(int direction)
        {
            var obj = lvSavedBuilds.Items[lvSavedBuilds.SelectedIndex];
            var selectedIndex = lvSavedBuilds.SelectedIndex;
            lvSavedBuilds.Items.RemoveAt(selectedIndex);
            lvSavedBuilds.Items.Insert(selectedIndex + direction, obj);
            lvSavedBuilds.SelectedItem = lvSavedBuilds.Items[selectedIndex + direction];
            lvSavedBuilds.SelectedIndex = selectedIndex + direction;
            lvSavedBuilds.Items.Refresh();

            SaveBuildsToFile();
        }

        #endregion

        #region Builds - Services
        private void SetCurrentBuild(PoEBuild build)
        {
            _persistentData.CurrentBuild = PoEBuild.Copy(build);

            tbSkillURL.Text = build.Url;
            tbLevel.Text = build.Level;
            LoadItemData(build.ItemData);
        }

        private void SaveNewBuild()
        {
            var formBuildName = new FormChooseBuildName(_persistentData.CurrentBuild.CharacterName, _persistentData.CurrentBuild.AccountName, _persistentData.CurrentBuild.ItemData);
            formBuildName.Owner = this;
            var show_dialog = formBuildName.ShowDialog();
            if (show_dialog != null && (bool)show_dialog)
            {
                var newBuild = new PoEBuild
                {
                    Name = formBuildName.GetBuildName(),
                    Level = tbLevel.Text,
                    Class = cbCharType.Text,
                    PointsUsed = tbUsedPoints.Text,
                    Url = tbSkillURL.Text,
                    Note = formBuildName.GetNote(),
                    CharacterName = formBuildName.GetCharacterName(),
                    AccountName = formBuildName.GetAccountName(),
                    ItemData = formBuildName.GetItemData(),
                    LastUpdated = DateTime.Now
                };
                SetCurrentBuild(newBuild);
                lvSavedBuilds.Items.Add(newBuild);
            }

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
        }

        private void SaveBuildsToFile()
        {
            _persistentData.SaveBuilds(lvSavedBuilds.Items);
        }

        private void LoadBuildFromUrl()
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(Tree, tbSkillURL.Text);
                    tbSkillURL.Text = Tree.SaveToURL();
                }
                else if (tbSkillURL.Text.Contains("tinyurl.com"))
                {
                    var request = (HttpWebRequest)WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse)request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    LoadBuildFromUrl();
                }
                else if (tbSkillURL.Text.Contains("poeurl.com"))
                {
                    tbSkillURL.Text = tbSkillURL.Text.Replace("http://poeurl.com/",
                        "http://poeurl.com/redirect.php?url=");
                    var request = (HttpWebRequest)WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse)request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    LoadBuildFromUrl();
                }
                else
                {
                    string[] urls = new string[] {
                        "https://poebuilder.com/character/",
                        "http://poebuilder.com/character/",
                        "https://www.poebuilder.com/character/",
                        "http://www.poebuilder.com/character/",
                        "https://www.pathofexile.com/fullscreen-passive-skill-tree/",
                        "http://www.pathofexile.com/fullscreen-passive-skill-tree/",
                        "https://pathofexile.com/fullscreen-passive-skill-tree/",
                        "http://pathofexile.com/fullscreen-passive-skill-tree/"
                    };
                    var urlString = tbSkillURL.Text;
                    foreach (string link in urls)
                    {
                        urlString = urlString.Replace(link, MainWindow.TreeAddress);
                    }
                    tbSkillURL.Text = urlString;
                    Tree.LoadFromURL(urlString);
                }


                _justLoaded = true;
                //cleans the default tree on load if 2
                if (_justLoaded)
                {
                    if (_undoList.Count > 1)
                    {
                        string holder = _undoList.Pop();
                        _undoList.Pop();
                        _undoList.Push(holder);
                    }
                }
                UpdateUI();
                _justLoaded = false;
            }
            catch (Exception)
            {
                MessageBox.Show(this, "The Build you tried to load, is invalid");
            }
        }
        #endregion

        #region Builds - DragAndDrop

        private void ListViewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragAndDropStartPoint = e.GetPosition(lvSavedBuilds);
        }

        private void ListViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(lvSavedBuilds);

                if (Math.Abs(position.X - _dragAndDropStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _dragAndDropStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    BeginDrag(e);
                }
            }
        }

        private void BeginDrag(MouseEventArgs e)
        {
            var listView = lvSavedBuilds;
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

            if (listViewItem == null)
                return;

            // get the data for the ListViewItem
            var item = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            //setup the drag adorner.
            InitialiseAdorner(listViewItem);

            //add handles to update the adorner.
            listView.PreviewDragOver += ListViewDragOver;
            listView.DragLeave += ListViewDragLeave;
            listView.DragEnter += ListViewDragEnter;

            var data = new DataObject("myFormat", item);
            DragDrop.DoDragDrop(lvSavedBuilds, data, DragDropEffects.Move);

            //cleanup 
            listView.PreviewDragOver -= ListViewDragOver;
            listView.DragLeave -= ListViewDragLeave;
            listView.DragEnter -= ListViewDragEnter;

            if (_adorner != null)
            {
                AdornerLayer.GetAdornerLayer(listView).Remove(_adorner);
                _adorner = null;
                SaveBuildsToFile();
            }
        }

        private void ListViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }


        private void ListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("myFormat"))
            {
                var name = e.Data.GetData("myFormat");
                ListView listView = lvSavedBuilds;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (listViewItem != null)
                {
                    var itemToReplace = listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    int index = listView.Items.IndexOf(itemToReplace);

                    if (index >= 0)
                    {
                        listView.Items.Remove(name);
                        listView.Items.Insert(index, name);
                    }
                }
                else
                {
                    listView.Items.Remove(name);
                    listView.Items.Add(name);
                }
            }
        }

        private void InitialiseAdorner(UIElement listViewItem)
        {
            var brush = new VisualBrush(listViewItem);
            _adorner = new DragAdorner(listViewItem, listViewItem.RenderSize, brush) { Opacity = 0.5 };
            _layer = AdornerLayer.GetAdornerLayer(lvSavedBuilds);
            _layer.Add(_adorner);
        }

        void ListViewDragLeave(object sender, DragEventArgs e)
        {
            if (Equals(e.OriginalSource, lvSavedBuilds))
            {
                var p = e.GetPosition(lvSavedBuilds);
                var r = VisualTreeHelper.GetContentBounds(lvSavedBuilds);
                if (!r.Contains(p))
                {
                    e.Handled = true;
                }
            }
        }

        void ListViewDragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.OffsetLeft = args.GetPosition(lvSavedBuilds).X - _dragAndDropStartPoint.X;
                _adorner.OffsetTop = args.GetPosition(lvSavedBuilds).Y - _dragAndDropStartPoint.Y;
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #endregion

        #region Bottom Bar (Build URL etc)

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchUpdate();
        }

        private void cbRegEx_Click(object sender, RoutedEventArgs e)
        {
            SearchUpdate();
        }

        private void SearchUpdate()
        {
            Tree.HighlightNodesBySearch(tbSearch.Text, cbRegEx.IsChecked != null && cbRegEx.IsChecked.Value, true);
        }

        private void ClearSearch()
        {
            tbSearch.Text = "";
            SearchUpdate();
        }

        private void tbSkillURL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoadBuildFromUrl();
        }

        private void tbSkillURL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbSkillURL.SelectAll();
        }

        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            _undoList.Push(tbSkillURL.Text);
        }

        private void tbSkillURL_Undo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Undo();
        }

        private void tbSkillURL_Undo()
        {
            if (_undoList.Count <= 0) return;
            if (_undoList.Peek() == tbSkillURL.Text && _undoList.Count > 1)
            {
                _undoList.Pop();
                tbSkillURL_Undo();
            }
            else if (_undoList.Peek() != tbSkillURL.Text)
            {
                _redoList.Push(tbSkillURL.Text);
                tbSkillURL.Text = _undoList.Pop();
                Tree.LoadFromURL(tbSkillURL.Text);
                UpdateUI();
            }
        }

        private void tbSkillURL_Redo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Redo();
        }

        private void tbSkillURL_Redo()
        {
            if (_redoList.Count <= 0) return;
            if (_redoList.Peek() == tbSkillURL.Text && _redoList.Count > 1)
            {
                _redoList.Pop();
                tbSkillURL_Redo();
            }
            else if (_redoList.Peek() != tbSkillURL.Text)
            {
                tbSkillURL.Text = _redoList.Pop();
                Tree.LoadFromURL(tbSkillURL.Text);
                UpdateUI();
            }
        }

        private void btnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            LoadBuildFromUrl();
        }

        private void btnPoeUrl_Click(object sender, RoutedEventArgs e)
        {
            StartDownloadPoeUrl();
        }

        private void StartDownloadPoeUrl()
        {
            var regx =
                new Regex(
                    "http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?",
                    RegexOptions.IgnoreCase);

            var matches = regx.Matches(tbSkillURL.Text);

            if (matches.Count == 1)
            {
                try
                {
                    var url = matches[0].ToString();
                    if (url.Length <= 12)
                    {
                        ShowPoeUrlMessageAndAddToClipboard(url);
                    }
                    if (!url.ToLower().StartsWith("http") && !url.ToLower().StartsWith("ftp"))
                    {
                        url = "http://" + url;
                    }
                    var client = new WebClient();
                    client.DownloadStringCompleted += DownloadCompletedPoeUrl;
                    client.DownloadStringAsync(new Uri("http://poeurl.com/shrink.php?url=" + url));
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Failed to create PoEURL", "poeurl error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DownloadCompletedPoeUrl(Object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, "Failed to create PoEURL", "poeurl error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ShowPoeUrlMessageAndAddToClipboard("http://poeurl.com/" + e.Result.Trim());
        }

        private void ShowPoeUrlMessageAndAddToClipboard(string poeurl)
        {
            System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
            MessageBox.Show(this, "The URL below has been copied to you clipboard: \n" + poeurl, "poeurl Link",
                MessageBoxButton.OK);
        }

        #endregion

        #region Theme

        private void mnuSetTheme_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            SetTheme(menuItem.Header as string);
        }

        private void SetTheme(string sTheme)
        {
            var accent = ThemeManager.Accents.First(x => Equals(x.Name, _persistentData.Options.Accent));
            var theme = ThemeManager.GetAppTheme("Base" + sTheme);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            ((MenuItem)NameScope.GetNameScope(this).FindName("mnuViewTheme" + sTheme)).IsChecked = true;
            _persistentData.Options.Theme = sTheme;
        }

        private void mnuSetAccent_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            SetAccent(menuItem.Header as string);
        }

        private void SetAccent(string sAccent)
        {
            var accent = ThemeManager.Accents.First(x => Equals(x.Name, sAccent));
            var theme = ThemeManager.GetAppTheme("Base" + _persistentData.Options.Theme);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            ((MenuItem)NameScope.GetNameScope(this).FindName("mnuViewAccent" + sAccent)).IsChecked = true;
            _persistentData.Options.Accent = sAccent;
        }
        #endregion

        #region Change Class - No Reset

        /**
         * Will get the current class name and start string from the tree url
         * return: array[]
         *         index 0 containing the Class Name
         *         index 1 containing the Class Start String
         **/
        private string[] getCurrentClass()
        {
            if (tbSkillURL.Text.IndexOf("AAAAAgAA") != -1)
            {
                return getAnyClass("Scion");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgEA") != -1)
            {
                return getAnyClass("Marauder");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgIA") != -1)
            {
                return getAnyClass("Ranger");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgMA") != -1)
            {
                return getAnyClass("Witch");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgQA") != -1)
            {
                return getAnyClass("Duelist");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgUA") != -1)
            {
                return getAnyClass("Templar");
            }
            else if (tbSkillURL.Text.IndexOf("AAAAAgYA") != -1)
            {
                return getAnyClass("Shadow");
            }
            else
            {
                return getAnyClass("ERROR");
            }
        }

        /**
         * parameters: className - any valid class name string
         * return: array[]
         *         index 0 containing the Class Name
         *         index 1 containing the Class Start String
         **/
        private string[] getAnyClass(string className)
        {
            string[] array = new string[2];
            if (className == "Scion")
            {
                array[0] = "Scion";
                array[1] = "AAAAAgAA";
                return array;
            }
            else if (className == "Marauder")
            {
                array[0] = "Marauder";
                array[1] = "AAAAAgEA";
                return array;
            }
            else if (className == "Ranger")
            {
                array[0] = "Ranger";
                array[1] = "AAAAAgIA";
                return array;
            }
            else if (className == "Witch")
            {
                array[0] = "Witch";
                array[1] = "AAAAAgMA";
                return array;
            }
            else if (className == "Duelist")
            {
                array[0] = "Duelist";
                array[1] = "AAAAAgQA";
                return array;
            }
            else if (className == "Templar")
            {
                array[0] = "Templar";
                array[1] = "AAAAAgUA";
                return array;
            }
            else if (className == "Shadow")
            {
                array[0] = "Shadow";
                array[1] = "AAAAAgYA";
                return array;
            }
            else
            {
                array[0] = "ERROR";
                array[1] = "ERROR";
                return array;
            }
        }
        #endregion

        #region Legacy

        /// <summary>
        /// Import builds from legacy build save file "savedBuilds" to PersistentData.xml.
        /// Warning: This will remove the "savedBuilds"
        /// </summary>
        private void ImportLegacySavedBuilds()
        {
            try
            {
                if (File.Exists("savedBuilds"))
                {
                    var saved_builds = new List<PoEBuild>();
                    var builds = File.ReadAllText("savedBuilds").Split('\n');
                    foreach (var b in builds)
                    {
                        var description = b.Split(';')[0].Split('|')[1];
                        var poeClass = description.Split(',')[0].Trim();
                        var pointsUsed = description.Split(',')[1].Trim().Split(' ')[0].Trim();

                        if (HasBuildNote(b))
                        {

                            saved_builds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], poeClass, pointsUsed,
                                b.Split(';')[1].Split('|')[0], b.Split(';')[1].Split('|')[1]));
                        }
                        else
                        {
                            saved_builds.Add(new PoEBuild(b.Split(';')[0].Split('|')[0], poeClass, pointsUsed,
                                b.Split(';')[1], ""));
                        }
                    }
                    lvSavedBuilds.Items.Clear();
                    foreach (var lvi in saved_builds)
                    {
                        lvSavedBuilds.Items.Add(lvi);
                    }
                    File.Move("savedBuilds", "savedBuilds.old");
                    SaveBuildsToFile();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unable to load the saved builds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool HasBuildNote(string b)
        {
            var buildNoteTest = b.Split(';')[1].Split('|');
            return buildNoteTest.Length > 1;
        }

        #endregion

        private void lvSavedBuilds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tree == null)
                return;

            if (lvSavedBuilds != null && lvSavedBuilds.SelectedItem is PoEBuild && _persistentData.Options.TreeComparisonEnabled)
            {
                var build = (PoEBuild)lvSavedBuilds.SelectedItem;
                HashSet<ushort> nodes;
                int ctype;
                SkillTree.DecodeURL(build.Url, out nodes, out ctype);

                Tree.HighlightedNodes = nodes;
                int level = 0;
                try
                {
                    level = int.Parse(build.Level);
                }
                catch
                {
                    level = 0;
                }
                Tree.HighlightedAttributes = SkillTree.GetAttributes(nodes, ctype, level);
            }
            else
            {
                Tree.HighlightedNodes = null;
                Tree.HighlightedAttributes = null;
            }

            Tree.DrawNodeBaseSurroundHighlight();
            UpdateAttributeList();
        }

        private void ToggleTreeComparison_Click(object sender, RoutedEventArgs e)
        {
            lvSavedBuilds_SelectionChanged(null, null);
        }
    }
}