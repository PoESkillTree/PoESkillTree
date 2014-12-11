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
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using POESKillTree.Controls;
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
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using ToolTip = System.Windows.Controls.ToolTip;

namespace POESKillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly PersistentData _persistentData = new PersistentData();
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

        public const string DefaultDataFolder = @".\Data";

        public MainWindow()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            InitializeComponent();
        }

        #region Window methods
        /*build the tree from the given dataFolder*/
        private void BuildTreeFromData(string dataFolder = DefaultDataFolder) 
        {
            Tree = SkillTree.CreateSkillTree(dataFolder, StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
            recSkillTree.Width = Tree.TRect.Width / Tree.TRect.Height * 500;
            recSkillTree.UpdateLayout();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

            Tree.Chartype =
                Tree.CharName.IndexOf(((string)((ComboBoxItem)cbCharType.SelectedItem).Content).ToUpper());
            Tree.UpdateAvailNodes();
            UpdateAllAttributeList();

            _multransform = Tree.TRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = Tree.TRect.TopLeft;
        }

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

            //Load Persistent Data and set theme
            _persistentData.LoadPersistentDataFromFile();
            SetTheme(_persistentData.Options.Theme);
            SetAccent(_persistentData.Options.Accent);

            expAttributes.IsExpanded = _persistentData.Options.AttributesBarOpened;
            expSavedBuilds.IsExpanded = _persistentData.Options.BuildsBarOpened;

            BuildTreeFromData(DefaultDataFolder);

            // loading last build
            if (_persistentData.CurrentBuild != null)
                SetCurrentBuild(_persistentData.CurrentBuild);

            LoadBuildFromUrl();
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
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _persistentData.CurrentBuild.Url = tbSkillURL.Text;
            _persistentData.CurrentBuild.Level = tbLevel.Text;
            _persistentData.Options.AttributesBarOpened = expAttributes.IsExpanded;
            _persistentData.Options.BuildsBarOpened = expSavedBuilds.IsExpanded;
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
            _loadingWindow.Show();
            Thread.Sleep(400);
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
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
            Tree.SkillAllHighligtedNodes();
            UpdateAllAttributeList();
            tbSkillURL.Text = Tree.SaveToURL();
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
            var diw = new DownloadItemsWindow(_persistentData.CurrentBuild.CharacterName) { Owner = this };
            diw.ShowDialog();
            _persistentData.CurrentBuild.CharacterName = diw.GetCharacterName();
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
            System.Windows.Forms.Clipboard.SetText(sb.ToString());
        }

        private void Menu_RedownloadTreeAssets(object sender, RoutedEventArgs e)
        {

            if (MessageBoxResult.Yes == MessageBox.Show(
                "You are about to re-download you tree data!\n Do you want to backup existing tree data?",
                "Tree data backup", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) ) 
            {
                Utils.IOHelper.BackupDirectory(DefaultDataFolder);
            }

            const string sMessageBoxText = "This will delete your " + DefaultDataFolder + 
                " folder and Redownload all the SkillTree assets.\nThis requires an internet connection!\n\nDo you want to proced?";
            const string sCaption = "Redownload SkillTree Assets - Warning";

            var rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    if (Directory.Exists(DefaultDataFolder))
                    {
                        try
                        {
                            string latestBackupName = Utils.IOHelper.GetLatestBackupFolder(DefaultDataFolder);
                            Directory.Delete(DefaultDataFolder, true);

                            BuildTreeFromData(DefaultDataFolder);
                            LoadBuildFromUrl();
                        }
                        catch (Exception)
                        {
                            CloseLoadingWindow();
                            try
                            {
                                string latestBackupName = Utils.IOHelper.GetLatestBackupFolder(DefaultDataFolder);
                                if (!latestBackupName.Equals(""))
                                {
                                    MessageBox.Show(@"Something went wrong! Switching to the latest backup (" + latestBackupName + ")",
                                        @"Oops!", MessageBoxButton.OK, MessageBoxImage.Error);

                                    // clean old folder
                                    Directory.Delete(DefaultDataFolder, true);

                                    //restore latest Data_backup 
                                    Utils.IOHelper.CopyDirectory(latestBackupName, DefaultDataFolder);
                                    LoadBuildFromUrl();
                                }
                                else
                                {
                                    MessageBox.Show(@"Now it's completely broken :( \n1.Close the program\n2.Delete the " + 
                                        DefaultDataFolder + @" folder\n3.Start the program again",
                                        @"Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            catch (Exception)
                            {
                                CloseLoadingWindow();
                                MessageBox.Show(@"Now it's completely broken :( \n1.Close the program\n2.Delete the " + 
                                    DefaultDataFolder + @" folder\n3.Start the program again",
                                    @"Ouch!", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
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

        private void Menu_OpenHotkeys(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new HotkeysWindow();
            aboutWindow.ShowDialog();
        }

        private void Menu_OpenAbout(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
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
                    // Show "No update is available" dialog.
                }
                else
                {
                    // Show dialog with release informations and "Install & Restart" button.
                }
            }
            catch (UpdaterException ex)
            {
                // Display error message: ex.Message.
            }
        }

        // Starts update process.
        private void btnUpdateInstall(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show download progress bar and Cancel button.
                // Start downloading.
                Updater.Download(UpdateDownloadCompleted, UpdateDownloadProgressChanged);
            }
            catch (UpdaterException ex)
            {
                // Display error message: ex.Message.
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
                }
            }
        }

        // Invoked when update download progress changes.
        private void UpdateDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update download progres bar.
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
            ComboBoxItem ComboItem = (ComboBoxItem)cbCharType.SelectedItem;
            string className = ComboItem.Name;

            if (Tree.canSwitchClass(className))
            {
                string[] currentClassArray = getCurrentClass();
                string[] changeClassArray = getAnyClass(className);

                if (currentClassArray[0] == "ERROR")
                    return;
                if (changeClassArray[0] == "ERROR")
                    return;
                string usedPoints = tbUsedPoints.Text;
                cbCharType.Text = changeClassArray[0];

                Tree.LoadFromURL(tbSkillURL.Text.Replace(currentClassArray[1], changeClassArray[1]));
                tbUsedPoints.Text = usedPoints;
            }
            else
            {
                SkillNode startnode =
                    Tree.Skillnodes.First(
                        nd => nd.Value.Name.ToUpper() == (Tree.CharName[cbCharType.SelectedIndex]).ToUpper()).Value;
                Tree.SkilledNodes.Clear();
                Tree.SkilledNodes.Add(startnode.Id);
                Tree.Chartype = Tree.CharName.IndexOf((Tree.CharName[cbCharType.SelectedIndex]).ToUpper());
            }
            Tree.UpdateAvailNodes();
            UpdateAllAttributeList();
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void tbLevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lvl;
            if (!int.TryParse(tbLevel.Text, out lvl)) return;
            Tree.Level = lvl;
            UpdateAllAttributeList();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (Tree == null)
                return;
            Tree.Reset();
            UpdateAllAttributeList();
            tbSkillURL.Text = Tree.SaveToURL();
        }

        #endregion

        #region Update Attribute and Character lists

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

                foreach (var a in Tree.ImplicitAttributes(attritemp))
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

            UpdateStatistics();

            UpdateAttributeList();
        }

        public void UpdateAttributeList()
        {
            _attiblist.Clear();
            foreach (var item in (Tree.SelectedAttributes.Select(InsertNumbersInAttributes)))
            {
                var a = new Attribute(item);
                _attiblist.Add(a);
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
            mnuViewAttributes.IsChecked = !mnuViewAttributes.IsChecked;
            expAttributes.IsExpanded = !expAttributes.IsExpanded;
        }

        private void ToggleAttributes_Click(object sender, RoutedEventArgs e)
        {
            ToggleAttributes();
        }

        private void expAttributes_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group collapsion events.
            {
                mnuViewAttributes.IsChecked = false;
            }
        }

        private void expAttributes_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group collapsion events.
            {
                mnuViewAttributes.IsChecked = true;

                if (expSheet.IsExpanded) expSheet.IsExpanded = false;
            }
        }

        private void TextBlock_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedAttr =
                Regex.Replace(
                    Regex.Match(listBox1.SelectedItem.ToString(), @"(?!\d)\w.*\w")
                        .Value.Replace(@"+", @"\+")
                        .Replace(@"-", @"\-")
                        .Replace(@"%", @"\%"), @"\d+", @"\d+");
            Tree.HighlightNodes(selectedAttr, true, Brushes.Azure);
        }

        private void expAttributes_MouseLeave(object sender, MouseEventArgs e)
        {
            SearchUpdate();
        }

        private void expSheet_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender == e.Source) // Ignore contained ListBox group expansion events.
            {
                if (expAttributes.IsExpanded) ToggleAttributes();
            }
        }

        private void ToggleBuilds()
        {
            mnuViewBuilds.IsChecked = !mnuViewBuilds.IsChecked;
            expSavedBuilds.IsExpanded = !expSavedBuilds.IsExpanded;
        }

        private void ToggleBuilds_Click(object sender, RoutedEventArgs e)
        {
            ToggleBuilds();
        }

        private void expSavedBuilds_Collapsed(object sender, RoutedEventArgs e)
        {
            mnuViewBuilds.IsChecked = false;
        }

        private void expSavedBuilds_Expanded(object sender, RoutedEventArgs e)
        {
            mnuViewBuilds.IsChecked = true;
        }

        #endregion

        #region zbSkillTreeBackground

        private void zbSkillTreeBackground_Click(object sender, RoutedEventArgs e)
        {
            Point p = ((MouseEventArgs) e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);

            v = v*_multransform + _addtransform;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
            {
                var node = nodes.First().Value;

                if (node.Spc == null)
                {
                    if (Tree.SkilledNodes.Contains(node.Id))
                    {
                        Tree.ForceRefundNode(node.Id);
                        UpdateAllAttributeList();

                        _prePath = Tree.GetShortestPathTo(node.Id);
                        Tree.DrawPath(_prePath);
                    }
                    else if (_prePath != null)
                    {
                        foreach (ushort i in _prePath)
                        {
                            Tree.SkilledNodes.Add(i);
                        }
                        UpdateAllAttributeList();
                        Tree.UpdateAvailNodes();

                        _toRemove = Tree.ForceRefundNodePreview(node.Id);
                        if (_toRemove != null)
                            Tree.DrawRefundPreview(_toRemove);
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
            v = v*_multransform + _addtransform;
            textBox1.Text = "" + v.X;
            textBox2.Text = "" + v.Y;
            SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                Tree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
                node = nodes.First().Value;

            if (node != null && node.Attributes.Count != 0)
            {
                var tooltip = node.Name + "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(_sToolTip.IsOpen && _lasttooltip == tooltip))
                {
                    _sToolTip.Content = tooltip;
                    _sToolTip.IsOpen = true;
                    _lasttooltip = tooltip;
                }
                if (Tree.SkilledNodes.Contains(node.Id))
                {
                    _toRemove = Tree.ForceRefundNodePreview(node.Id);
                    if (_toRemove != null)
                        Tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = Tree.GetShortestPathTo(node.Id);
                    Tree.DrawPath(_prePath);
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

        private void zbSkillTreeBackground_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            zbSkillTreeBackground.Child.RaiseEvent(e);
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
                    MessageBox.Show("Item data currupted!");
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

            UpdateAllAttributeList();
        }

        public void ClearCurrentItemData()
        {
            _persistentData.CurrentBuild.ItemData = "";
            _itemAttributes = null;
            lbItemAttr.ItemsSource = null;
            UpdateAllAttributeList();
            mnuClearItems.IsEnabled = false;
        }

        #endregion

        #region Builds - Event Handlers

        private void lvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lvi = ((ListView)sender).SelectedItem;
            if (lvi == null) return;
            var build = ((PoEBuild)lvi);
            SetCurrentBuild(build);
            LoadBuildFromUrl(); // loading the build
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
                _noteTip.Content = build.Note == @"" ? @"Right Click To Edit" : build.Note;
                _noteTip.IsOpen = true;
            }
        }

        private void lvi_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedBuild = (PoEBuild)lvSavedBuilds.SelectedItem;
            var formBuildName = new FormChooseBuildName(selectedBuild.Name, selectedBuild.Note, selectedBuild.CharacterName, selectedBuild.ItemData);
            var show_dialog = formBuildName.ShowDialog();
            if (show_dialog != null && (bool)show_dialog)
            {
                selectedBuild.Name = formBuildName.GetBuildName();
                selectedBuild.Note = formBuildName.GetNote();
                selectedBuild.CharacterName = formBuildName.GetCharacterName();
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
                selectedBuild.Level = tbLevel.Text;
                selectedBuild.PointsUsed = tbUsedPoints.Text;
                selectedBuild.Url = tbSkillURL.Text;
                selectedBuild.ItemData = _persistentData.CurrentBuild.ItemData;
                lvSavedBuilds.Items.Refresh();
                SaveBuildsToFile();
            }
            else
            {
                MessageBox.Show("Please select an existing build first.", "Error", MessageBoxButton.OK,
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
            var formBuildName = new FormChooseBuildName(_persistentData.CurrentBuild.CharacterName, _persistentData.CurrentBuild.ItemData);
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
                    ItemData = formBuildName.GetItemData()
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
                else if (tbSkillURL.Text.Contains("poebuilder.com/"))
                {
                    const string poebuilderTree = "https://poebuilder.com/character/";
                    const string poebuilderTreeWWW = "https://www.poebuilder.com/character/";
                    const string poebuilderTreeOWWW = "http://www.poebuilder.com/character/";
                    const string poebuilderTreeO = "http://poebuilder.com/character/";
                    var urlString = tbSkillURL.Text;
                    urlString = urlString.Replace(poebuilderTree, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeO, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeWWW, MainWindow.TreeAddress);
                    urlString = urlString.Replace(poebuilderTreeOWWW, MainWindow.TreeAddress);
                    tbSkillURL.Text = urlString;
                    Tree.LoadFromURL(urlString);
                }
                else if (tbSkillURL.Text.Contains("tinyurl.com/"))
                {
                    var request = (HttpWebRequest)WebRequest.Create(tbSkillURL.Text);
                    request.AllowAutoRedirect = false;
                    var response = (HttpWebResponse)request.GetResponse();
                    var redirUrl = response.Headers["Location"];
                    tbSkillURL.Text = redirUrl;
                    LoadBuildFromUrl();
                }
                else if (tbSkillURL.Text.Contains("poeurl.com/"))
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
                    Tree.LoadFromURL(tbSkillURL.Text);

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
                cbCharType.SelectedIndex = Tree.Chartype;
                UpdateAllAttributeList();
                _justLoaded = false;
            }
            catch (Exception)
            {
                MessageBox.Show("The Build you tried to load, is invalid");
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
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject) e.OriginalSource);

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
            _adorner = new DragAdorner(listViewItem, listViewItem.RenderSize, brush) {Opacity = 0.5};
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
            Tree.HighlightNodes(tbSearch.Text, cbRegEx.IsChecked != null && cbRegEx.IsChecked.Value);
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
                tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
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
                tbUsedPoints.Text = "" + (Tree.SkilledNodes.Count - 1);
            }
        }

        private void btnLoadBuild_Click(object sender, RoutedEventArgs e)
        {
            LoadBuildFromUrl();
        }

        private void btnPoeUrl_Click(object sender, RoutedEventArgs e)
        {
            string poeurl = "http://poeurl.com/" + ToPoeURLS(tbSkillURL.Text);
            System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
            MessageBox.Show("The URL below has been copied to you clipboard: \n" + poeurl, "poeurl Link",
                MessageBoxButton.OK);
        }

        protected string ToPoeURLS(string txt)
        {
            var regx =
                new Regex(
                    "http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?",
                    RegexOptions.IgnoreCase);

            MatchCollection mactches = regx.Matches(txt);

            foreach (Match match in mactches)
            {
                string pURL = MakePoeUrl(match.Value);
                txt = txt.Replace(match.Value, pURL);
            }

            return txt;
        }

        private static string MakePoeUrl(string url)
        {
            try
            {
                if (url.Length <= 12)
                {
                    return url;
                }
                if (!url.ToLower().StartsWith("http") && !url.ToLower().StartsWith("ftp"))
                {
                    url = "http://" + url;
                }
                var request = WebRequest.Create("http://poeurl.com/shrink.php?url=" + url);
                var res = request.GetResponse();
                string text;
                using (var reader = new StreamReader(res.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch (Exception)
            {
                return url;
            }
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
                MessageBox.Show("Unable to load the saved builds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool HasBuildNote(string b)
        {
            var buildNoteTest = b.Split(';')[1].Split('|');
            return buildNoteTest.Length > 1;
        }

        #endregion

        private void image1_LostFocus(object sender, MouseEventArgs e)
        {
            _sToolTip.IsOpen = false;
        }
    }
}