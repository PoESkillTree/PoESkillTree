using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.ViewModels;
using POESKillTree.TreeGenerator.Views;
using POESKillTree.Utils;
using POESKillTree.Utils.Converter;
using POESKillTree.ViewModels;
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
using POESKillTree.ViewModels.Items;

using Path = System.IO.Path;

namespace POESKillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        /// <summary>
        /// The set of keys of which one needs to be pressed to highlight similar nodes on hover.
        /// </summary>
        private static readonly Key[] HighlightByHoverKeys = { Key.LeftShift, Key.RightShift };

        private readonly PersistentData _persistentData = App.PersistentData;

        public event PropertyChangedEventHandler PropertyChanged;

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
        private ListCollectionView _attributeCollection;
        private ListCollectionView _defenceCollection;
        private ListCollectionView _offenceCollection;
        private RenderTargetBitmap _clipboardBmp;

        private GroupStringConverter _attributeGroups;
        private ContextMenu _attributeContextMenu;
        private MenuItem cmCreateGroup, cmAddToGroup, cmRemoveFromGroup, cmDeleteGroup;

        private ItemAttributes _itemAttributes;

        public ItemAttributes ItemAttributes
        {
            get { return _itemAttributes; }
            private set
            {
                _itemAttributes = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ItemAttributes"));
            }
        }

        private SkillTree _tree;

        public SkillTree Tree
        {
            get { return _tree; }
            private set
            {
                _tree = value;
                value.MainWindow = this;
                LevelUpDown.DataContext = _tree;
            }
        }

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

        /// <summary>
        /// The node of the SkillTree that currently has the mouse over it.
        /// Null if no node is under the mouse.
        /// </summary>
        private SkillNode _hoveredNode;

        private SkillNode _lastHoveredNode;

        private bool _noAsyncTaskRunning = true;
        /// <summary>
        /// Specifies if there is a task running asynchronously in the background.
        /// Used to disable UI buttons that might interfere with the result of the task.
        /// </summary>
        public bool NoAsyncTaskRunning
        {
            get { return _noAsyncTaskRunning; }
            private set
            {
                _noAsyncTaskRunning = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("NoAsyncTaskRunning"));
                }
            }
        }

        private SettingsWindow _settingsWindow;

        private bool _isClosing;

        private const string MainWindowTitle = "Path of Exile: Passive Skill Tree Planner";

        public MainWindow()
        {
            InitializeComponent();
        }

        //This whole region, along with most of GroupStringConverter, makes up our user-defined attribute group functionality - Sectoidfodder 02/29/16
        #region Attribute grouping helpers

        private void IgnoreRightClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        //there's probably a better way that doesn't break if tab ordering changes but I'm UI-challenged
        private ListBox GetActiveAttributeGroupList()
        {
            if (tabControl1.SelectedIndex == 2)
                return lbAllAttr;
            else if (tabControl1.SelectedIndex == 0)
                return listBox1;
            else
                return null;
        }

        //Necessary to update the summed numbers in group names before every refresh
        private void RefreshAttributeLists()
        {
            
            if (GetActiveAttributeGroupList()==lbAllAttr)
            {
                _attributeGroups.UpdateGroupNames(_allAttributesList);
            }
            //use passive attribute list as a default so nothing breaks if neither tab is actually active
            else
            {
                _attributeGroups.UpdateGroupNames(_attiblist);
            }
            _attributeCollection.Refresh();
            _allAttributeCollection.Refresh();
        }

        private void SetCustomGroups(List<string[]> customgroups)
        {
            cmAddToGroup.Items.Clear();
            cmDeleteGroup.Items.Clear();

            List<string> groupnames = new List<string>();
            MenuItem newSubMenu;

            foreach (var gp in customgroups)
            {
                if (!groupnames.Contains(gp[1]))
                {
                    groupnames.Add(gp[1]);
                }
            }

            cmAddToGroup.IsEnabled = false;
            cmDeleteGroup.IsEnabled = false;

            foreach (string name in groupnames)
            {
                newSubMenu = new MenuItem();
                newSubMenu.Header = name;
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem();
                newSubMenu.Header = name;
                newSubMenu.Click += DeleteGroup;
                cmDeleteGroup.Items.Add(newSubMenu);
                cmDeleteGroup.IsEnabled = true;
            }

            _attributeGroups.ResetGroups(customgroups);
            RefreshAttributeLists();
        }

        //Adds currently selected attributes to a new group
        private void CreateGroup(object sender, RoutedEventArgs e)
        {
            ListBox lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            List<string> attributelist = new List<string>();
            foreach (object o in lb.SelectedItems)
            {
                attributelist.Add(o.ToString());
            }

            //Build and show form to enter group name
            var formGroupName = new FormChooseGroupName();
            formGroupName.Owner = this;
            var show_dialog = formGroupName.ShowDialog();
            if (show_dialog != null && (bool)show_dialog)
            {
                string name = formGroupName.GetGroupName();
                if (_attributeGroups.AttributeGroups.ContainsKey(name))
                {
                    Popup.Info(L10n.Message("A group with that name already exists."));
                    return;
                }

                //Add submenus that add to and delete the new group
                MenuItem newSubMenu = new MenuItem();
                newSubMenu.Header = name;
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem();
                newSubMenu.Header = name;
                newSubMenu.Click += DeleteGroup;
                cmDeleteGroup.Items.Add(newSubMenu);
                cmDeleteGroup.IsEnabled = true;

                //Back end - actually make the new group
                _attributeGroups.AddGroup(name, attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Removes currently selected attributes from their custom groups, restoring them to their default groups
        private void RemoveFromGroup(object sender, RoutedEventArgs e)
        {
            ListBox lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            List<string> attributelist = new List<string>();
            foreach (object o in lb.SelectedItems)
            {
                attributelist.Add(o.ToString());
            }
            if (attributelist.Count > 0)
            {
                _attributeGroups.RemoveFromGroup(attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Adds currently selected attributes to an existing custom group named by sender.Header
        private void AddToGroup(object sender, RoutedEventArgs e)
        {
            ListBox lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            List<string> attributelist = new List<string>();
            foreach (object o in lb.SelectedItems)
            {
                attributelist.Add(o.ToString());
            }
            if (attributelist.Count > 0)
            {
                _attributeGroups.AddGroup(((MenuItem)sender).Header.ToString(), attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Deletes the entire custom group named by sender.Header, restoring all contained attributes to their default groups
        private void DeleteGroup(object sender, RoutedEventArgs e)
        {
            //Remove submenus that work with the group
            for (int i = 0; i < cmAddToGroup.Items.Count; i++)
            {
                if (((MenuItem)cmAddToGroup.Items[i]).Header.ToString().ToLower().Equals(((MenuItem)sender).Header.ToString().ToLower()))
                {
                    cmAddToGroup.Items.RemoveAt(i);
                    if (cmAddToGroup.Items.Count == 0)
                        cmAddToGroup.IsEnabled = false;
                    break;
                }
            }
            for (int i = 0; i < cmDeleteGroup.Items.Count; i++)
            {
                if (((MenuItem)cmDeleteGroup.Items[i]).Header.ToString().ToLower().Equals(((MenuItem)sender).Header.ToString().ToLower()))
                {
                    cmDeleteGroup.Items.RemoveAt(i);
                    if (cmDeleteGroup.Items.Count == 0)
                        cmDeleteGroup.IsEnabled = false;
                    break;
                }
            }

            _attributeGroups.DeleteGroup(((MenuItem)sender).Header.ToString());
            RefreshAttributeLists();
        }

        #endregion

        #region Window methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ItemDB.Load("Items.xml");
            ItemDB.Merge("ItemsLocal.xml");
            ItemDB.Index();

            var cmHighlight = new MenuItem
            {
                Header = L10n.Message("Highlight nodes by attribute")
            };
            cmHighlight.Click += HighlightNodesByAttribute;
            var cmRemoveHighlight = new MenuItem
            {
                Header = L10n.Message("Remove highlights by attribute")
            };
            cmRemoveHighlight.Click += UnhighlightNodesByAttribute;
            cmCreateGroup = new MenuItem();
            cmCreateGroup.Header = "Create new group";
            cmCreateGroup.Click += CreateGroup;
            cmAddToGroup = new MenuItem();
            cmAddToGroup.Header = "Add to group...";
            cmAddToGroup.IsEnabled = false;
            cmDeleteGroup = new MenuItem();
            cmDeleteGroup.Header = "Delete group...";
            cmDeleteGroup.IsEnabled = false;
            cmRemoveFromGroup = new MenuItem();
            cmRemoveFromGroup.Header = "Remove from group";
            cmRemoveFromGroup.Click += RemoveFromGroup;

            _attributeGroups = new GroupStringConverter();
            _attributeContextMenu = new ContextMenu();
            _attributeContextMenu.Items.Add(cmHighlight);
            _attributeContextMenu.Items.Add(cmRemoveHighlight);
            _attributeContextMenu.Items.Add(cmCreateGroup);
            _attributeContextMenu.Items.Add(cmAddToGroup);
            _attributeContextMenu.Items.Add(cmDeleteGroup);
            _attributeContextMenu.Items.Add(cmRemoveFromGroup);

            _attributeCollection = new ListCollectionView(_attiblist);
            _attributeCollection.GroupDescriptions.Add(new PropertyGroupDescription("Text", _attributeGroups));
            _attributeCollection.CustomSort = _attributeGroups;
            listBox1.ItemsSource = _attributeCollection;
            listBox1.SelectionMode = SelectionMode.Extended;
            listBox1.ContextMenu = _attributeContextMenu;

            _allAttributeCollection = new ListCollectionView(_allAttributesList);
            _allAttributeCollection.GroupDescriptions.Add(new PropertyGroupDescription("Text", _attributeGroups));
            _allAttributeCollection.CustomSort = _attributeGroups;
            lbAllAttr.ItemsSource = _allAttributeCollection;
            lbAllAttr.SelectionMode = SelectionMode.Extended;
            lbAllAttr.ContextMenu = _attributeContextMenu;

            _defenceCollection = new ListCollectionView(_defenceList);
            _defenceCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            listBoxDefence.ItemsSource = _defenceCollection;

            _offenceCollection = new ListCollectionView(_offenceList);
            _offenceCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            listBoxOffence.ItemsSource = _offenceCollection;

            cbCharType.ItemsSource =
                CharacterNames.NameToContent.Select(
                    x => new ComboBoxItem {Name = x.Key, Content = x.Value});
            cbAscType.SelectedIndex = 0;

            if (_persistentData.StashBookmarks != null)
                Stash.Bookmarks = new System.Collections.ObjectModel.ObservableCollection<StashBookmark>(_persistentData.StashBookmarks);

            // Set theme & accent.
            SetTheme(_persistentData.Options.Theme);
            SetAccent(_persistentData.Options.Accent);

            Tree = SkillTree.CreateSkillTree(StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
            recSkillTree.Width = SkillTree.TRect.Width / SkillTree.TRect.Height * recSkillTree.Height;
            recSkillTree.UpdateLayout();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

            _multransform = SkillTree.TRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = SkillTree.TRect.TopLeft;

            _justLoaded = true;

            // loading last build
            if (_persistentData.CurrentBuild != null)
                SetCurrentBuild(_persistentData.CurrentBuild);
            else
                LoadItemData(null);

            LoadBuildFromUrl();
            _justLoaded = false;
            // loading saved build
            lvSavedBuilds.Items.Clear();
            foreach (var build in _persistentData.Builds)
            {
                lvSavedBuilds.Items.Add(build);
            }
            CheckAppVersionAndDoNecessaryChanges();
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
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D2:
                        cbCharType.SelectedIndex = 1;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D3:
                        cbCharType.SelectedIndex = 2;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D4:
                        cbCharType.SelectedIndex = 3;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D5:
                        cbCharType.SelectedIndex = 4;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D6:
                        cbCharType.SelectedIndex = 5;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D7:
                        cbCharType.SelectedIndex = 6;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.Z:
                        tbSkillURL_Undo();
                        break;
                    case Key.Y:
                        tbSkillURL_Redo();
                        break;
                    case Key.S:
                        SaveBuild();
                        break;
                    case Key.N:
                        NewBuild();
                        break;
                }
            }

            if (HighlightByHoverKeys.Any(key => key == e.Key))
            {
                HighlightNodesByHover();
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

            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                switch (e.Key)
                {
                    case Key.S:
                        SaveNewBuild();
                        break;
                }
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (HighlightByHoverKeys.Any(key => key == e.Key))
            {
                HighlightNodesByHover();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _isClosing = true;

            _persistentData.CurrentBuild.Url = tbSkillURL.Text;
            _persistentData.CurrentBuild.Level = GetLevelAsString();
            _persistentData.SetBuilds(lvSavedBuilds.Items);
            _persistentData.StashBookmarks = Stash.Bookmarks.ToList();

            if (_settingsWindow != null)
            {
                _settingsWindow.Close();
            }
        }

        #endregion

        #region LoadingWindow

        private void StartLoadingWindow(string infoText)
        {
            _loadingWindow = new LoadingWindow() {Owner = this, InfoText = infoText};
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            _loadingWindow.Show();
        }

        private void UpdateLoadingWindow(double c, double max)
        {
            _loadingWindow.MaxProgress = max;
            _loadingWindow.Progress = c;
            _loadingWindow.UpdateLayout();
            _loadingWindow.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            if (Equals(c, max))
                Thread.Sleep(100);
        }

        private void CloseLoadingWindow()
        {
            _loadingWindow.Close();
        }

        #endregion

        #region Utility
        private void SetTitle(string buildName)
        {
            Title = buildName + " - " + MainWindowTitle;
        }
        #endregion

        #region Menu
        
        private void Menu_NewBuild(object sender, RoutedEventArgs e)
        {
            NewBuild();
        }

        private void Menu_SkillTaggedNodes(object sender, RoutedEventArgs e)
        {
            var currentCursor = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                Tree.SkillAllTaggedNodes();
                UpdateUI();
                tbSkillURL.Text = Tree.SaveToURL();
            }
            finally
            {
                Cursor = currentCursor;
            }
        }

        private void Menu_UntagAllNodes(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show(L10n.Message("Are you sure?"), L10n.Message("Untag All Skill Nodes"), MessageBoxButton.YesNo, MessageBoxImage.None, MessageBoxResult.No);
            if(response == MessageBoxResult.Yes)
                Tree.UntagAllNodes();
        }

        private void Menu_UnhighlightAllNodes(object sender, RoutedEventArgs e)
        {
            Tree.UnhighlightAllNodes();
            ClearSearch();
        }

        private void Menu_CheckAllHighlightedNodes(object sender, RoutedEventArgs e)
        {
            Tree.CheckAllHighlightedNodes();
        }

        private void Menu_CrossAllHighlightedNodes(object sender, RoutedEventArgs e)
        {
            Tree.CrossAllHighlightedNodes();
        }

        private void Menu_OpenTreeGenerator(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_settingsWindow == null)
                {
                    var vm = new SettingsViewModel(Tree);
                    vm.RunFinished += (o, args) =>
                    {
                        UpdateUI();
                        tbSkillURL.Text = Tree.SaveToURL();
                    };
                    vm.StartController += (o, args) =>
                    {
                        var dialog = new ControllerWindow() {Owner = this, DataContext = args.ViewModel};
                        dialog.ShowDialog();
                    };
                    _settingsWindow = new SettingsWindow() {Owner = this, DataContext = vm};
                    _settingsWindow.Closing += (o, args) =>
                    {
                        if (_isClosing) return;
                        args.Cancel = true;
                        _settingsWindow.Hide();
                    };
                }
                _settingsWindow.Show();
            }
            catch (Exception ex)
            {
                Popup.Error(L10n.Message("Could not open Skill Tree Generator"), ex.Message);
                Debug.WriteLine("Exception in 'Skill Tree Generator':");
                Debug.WriteLine(ex.Message);
            }
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

                //Save image in clipboard
                Clipboard.SetImage(_clipboardBmp);

                //Convert renderTargetBitmap to bitmap
                MemoryStream stream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_clipboardBmp));
                encoder.Save(stream);

                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);

                // Configure save file dialog box
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();

                // Default file name -- current build name ("buildname - xxx points used")
                uint skilledNodes = (uint) Tree.GetPointCount()["NormalUsed"];
                dialog.FileName = PersistentData.CurrentBuild.Name + " - " + string.Format(L10n.Plural("{0} point", "{0} points", skilledNodes), skilledNodes);

                dialog.DefaultExt = ".jpg"; // Default file extension
                dialog.Filter = "JPEG (*.jpg, *.jpeg)|*.jpg;|PNG (*.png)|*.png"; // Filter files by extension
                dialog.OverwritePrompt = true;

                // Show save file dialog box
                bool? result = dialog.ShowDialog();

                // Continue if the user did select a path
                if (result.HasValue && result == true)
                {
                    System.Drawing.Imaging.ImageFormat format;
                    string fileExtension = System.IO.Path.GetExtension(dialog.FileName);

                    //set the selected data type
                    switch (fileExtension)
                    {
                        case ".png":
                            format = System.Drawing.Imaging.ImageFormat.Png;
                            break;

                        case ".jpg":
                        case ".jpeg":
                        default:
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                    }

                    //save the file
                    image.Save(dialog.FileName, format);
                }

                recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
            }
            else
            {
                MessageBox.Show(L10n.Message("Your build must use at least one node to generate a screenshot"), "Screenshot Generator", MessageBoxButton.OK, MessageBoxImage.Information);
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
            catch (Exception ex)
            {
                Popup.Error(L10n.Message("An error occurred while copying to Clipboard."), ex.Message);
            }
        }

        private void Menu_RedownloadTreeAssets(object sender, RoutedEventArgs e)
        {
            string sMessageBoxText = L10n.Message("The existing Skill tree assets will be deleted and new assets will be downloaded.")
                                     + "\n\n" + L10n.Message("Do you want to continue?");

            var rsltMessageBox = Popup.Ask(sMessageBoxText, MessageBoxImage.Warning);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    string appDataPath = AppData.GetFolder(true);

                    try
                    {
                        if (Directory.Exists(appDataPath + "Data"))
                        {
                            if (Directory.Exists(appDataPath + "DataBackup"))
                                Directory.Delete(appDataPath + "DataBackup", true);

                            Directory.Move(appDataPath + "Data", appDataPath + "DataBackup");
                        }

                        Tree = SkillTree.CreateSkillTree(StartLoadingWindow, UpdateLoadingWindow, CloseLoadingWindow);
                        recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);


                        SkillTree.ClearAssets();//enable recaching of assets
                        SkillTree.CreateSkillTree();//create new skilltree to reinitialize cache


                        LoadBuildFromUrl();
                        _justLoaded = false;

                        if (Directory.Exists(appDataPath + "DataBackup"))
                            Directory.Delete(appDataPath + "DataBackup", true);
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists(appDataPath + "Data") && Directory.Exists(appDataPath + "DataBackup"))
                            Directory.Delete(appDataPath + "Data", true);
                        try
                        {
                            CloseLoadingWindow();
                        }
                        catch (Exception)
                        {
                            //Nothing
                        }
                        if (Directory.Exists(appDataPath + "DataBackup"))
                            Directory.Move(appDataPath + "DataBackup", appDataPath + "Data");

                        Popup.Error(L10n.Message("An error occurred while downloading assets."), ex.Message);
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

        private void Menu_OpenSettings(object sender, RoutedEventArgs e)
        {
            var settingsWindows = new SettingsMenuWindow() { Owner = this };
            settingsWindows.ShowDialog();
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
        private async void Menu_CheckForUpdates(object sender, RoutedEventArgs e)
        {
            try
            {
                // No non-Task way without rewriting Updater to support/use await directly.
                var release =
                    await AwaitAsyncTask(L10n.Message("Checking for updates"),
                        Task.Run(() => Updater.CheckForUpdates()));

                if (release == null)
                {
                    Popup.Info(L10n.Message("You have the latest version!"));
                }
                else
                {
                    string message = release.IsUpdate
                        ? string.Format(L10n.Message("An update for {0} ({1}) is available!"),
                            Properties.Version.ProductName, release.Version)
                          + "\n\n" +
                          L10n.Message("The application will be closed when download completes to proceed with the update.")
                        : string.Format(L10n.Message("A new version {0} is available!"), release.Version)
                          + "\n\n" +
                          L10n.Message(
                              "The new version of application will be installed side-by-side with earlier versions.");

                    if (release.IsPrerelease)
                        message += "\n\n" +
                                   L10n.Message("Warning: This is a pre-release, meaning there could be some bugs!");

                    message += "\n\n" +
                               (release.IsUpdate
                                   ? L10n.Message("Do you want to download and install the update?")
                                   : L10n.Message("Do you want to download and install the new version?"));

                    MessageBoxResult download = Popup.Ask(message,
                        release.IsPrerelease ? MessageBoxImage.Warning : MessageBoxImage.Question);
                    if (download == MessageBoxResult.Yes)
                        btnUpdateInstall();
                    else
                        btnUpdateCancel();
                }
            }
            catch (UpdaterException ex)
            {
                Popup.Error(
                    L10n.Message("An error occurred while attempting to contact the update location."),
                    ex.Message);
            }
        }

        // Starts update process.
        private void btnUpdateInstall()
        {
            try
            {
                StartLoadingWindow(L10n.Message("Downloading latest version"));
                Updater.Download(UpdateDownloadCompleted, UpdateDownloadProgressChanged);
            }
            catch (UpdaterException ex)
            {
                Popup.Error(L10n.Message("An error occurred during the download operation."), ex.Message);
            }
        }

        // Cancels update download (also invoked when download progress dialog is closed).
        private void btnUpdateCancel()
        {
            if (Updater.IsDownloading)
                Updater.Cancel();
            else
            {
                Updater.Dispose();
            }
        }

        // Invoked when update download completes, aborts or fails.
        private void UpdateDownloadCompleted(Object sender, AsyncCompletedEventArgs e)
        {
            CloseLoadingWindow();
            if (e.Cancelled) // Check whether download was cancelled.
            {
                Updater.Dispose();
            }
            else if (e.Error != null) // Check whether error occurred.
            {
                Popup.Error(L10n.Message("An error occurred during the download operation."), e.Error.Message);
            }
            else // Download completed.
            {
                try
                {
                    Updater.Install();
                    // Release being installed is an update, we have to exit application.
                    if (Updater.GetLatestRelease().IsUpdate) App.Current.Shutdown();
                }
                catch (UpdaterException ex)
                {
                    Updater.Dispose();
                    Popup.Error(L10n.Message("An error occurred while attempting to start the installation."), ex.Message);
                }
            }
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
             if (Tree == null)
                return;

            if (_justLoaded)
            {
                _justLoaded = false;
                populateAsendancySelectionList();
                return;
            }

            var ComboItem = (ComboBoxItem)cbCharType.SelectedItem;
            var className = ComboItem.Name;

            if (Tree.CanSwitchClass(className))
            {
                HashSet<ushort> tempNodes = new HashSet<ushort>(Tree.SkilledNodes);
                foreach(int i in SkillTree.rootNodeList)
                {
                    if (tempNodes.Contains((ushort)i))
                        tempNodes.Remove((ushort)i);
                }
                Tree.Chartype = cbCharType.SelectedIndex;
                if (cbAscType.SelectedIndex != -1)
                    Tree.AscType = cbAscType.SelectedIndex;
                Tree.SkilledNodes.UnionWith(tempNodes);
            }
            else
            {
                Tree.Chartype = cbCharType.SelectedIndex;
                Tree.AscType = cbAscType.SelectedIndex;
            }

            if (Tree.updateAscendancyClasses)
            {
                populateAsendancySelectionList();
            }
            Tree.UpdateAvailNodes();
            UpdateUI();
            tbSkillURL.Text = Tree.SaveToURL();
        }

        private void populateAsendancySelectionList()
        {
            List<string> ascendancyItems = new List<string>();
            ascendancyItems.Add("None");
            foreach (var name in Tree.ascendancyClasses.GetClasses(((ComboBoxItem)cbCharType.SelectedItem).Content.ToString()))
                ascendancyItems.Add(name.displayName);
            cbAscType.ItemsSource = ascendancyItems.Select(x => new ComboBoxItem { Name = x, Content = x });
            Tree.updateAscendancyClasses = false;
            cbAscType.SelectedIndex = Tree.AscType;
        }

        private string GetLevelAsString()
        {
            return Tree.Level.ToString(CultureInfo.CurrentCulture);
        }

        private void SetLevelFromString(string s)
        {
            int lvl;
            if (int.TryParse(s, out lvl))
            {
                Tree.Level = lvl;
            }
        }

        private void Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> args)
        {
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
            UpdateAllAttributeList();
            RefreshAttributeLists();
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

        }

        public void UpdateClass()
        {
            if (Tree.Chartype != -1)
                cbCharType.SelectedIndex = Tree.Chartype;
            if (Tree.AscType != -1)
                cbAscType.SelectedIndex = Tree.AscType;
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
            UpdatePoints();
        }
        public void UpdatePoints()
        {
            Dictionary<string, int> points = Tree.GetPointCount();
            NormalUsedPoints.Content = points["NormalUsed"].ToString();
            NormalTotalPoints.Content = points["NormalTotal"].ToString();
            AscendancyUsedPoints.Content = "[" + points["AscendancyUsed"].ToString() + "]";
            AscendancyTotalPoints.Content = "[" + points["AscendancyTotal"].ToString() + "]";
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

        private void HighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            var listBox = _attributeContextMenu.PlacementTarget as ListBox;
            if (listBox == null || !listBox.IsVisible) return;

            var newHighlightedAttribute =
                "^" + Regex.Replace(listBox.SelectedItem.ToString()
                        .Replace(@"+", @"\+")
                        .Replace(@"-", @"\-")
                        .Replace(@"%", @"\%"), @"[0-9]*\.?[0-9]+", @"[0-9]*\.?[0-9]+") + "$";
            Tree.HighlightNodesBySearch(newHighlightedAttribute, true, NodeHighlighter.HighlightState.FromAttrib);
        }

        private void UnhighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            Tree.HighlightNodesBySearch("", true, NodeHighlighter.HighlightState.FromAttrib);
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
                if (node.ascendancyName != null && !Tree.drawAscendancy)
                    return;
                string className = CharacterNames.NameToContent.Where(x => x.Key == SkillTree.CharName[Tree.Chartype]).First().Value;
                if (!_persistentData.Options.ShowAllAscendancyClasses && node.ascendancyName != Tree.ascendancyClasses.GetClassName(className, Tree.AscType))
                    return;

                if (node.Spc == null && !node.IsMastery)
                {
                    if (_lastMouseButton == MouseButton.Right)
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        {
                            // Backward on shift+RMB
                            Tree.CycleNodeTagBackward(node);
                        }
                        else
                        {
                            // Forward on RMB
                            Tree.CycleNodeTagForward(node);
                        }
                        e.Handled = true;
                    }
                    else
                    {
                        // Toggle whether the node is included in the tree
                        if (Tree.SkilledNodes.Contains(node.Id))
                        {
                            Tree.ForceRefundNode(node.Id);
                            _prePath = Tree.GetShortestPathTo(node.Id, Tree.SkilledNodes);
                            Tree.DrawPath(_prePath);
                        }
                        else if (_prePath != null)
                        {
                            foreach (ushort i in _prePath)
                            {
                                var temp = SkillTree.Skillnodes[i];
                                if (temp.IsMultipleChoiceOption)
                                {
                                    foreach(var j in Tree.SkilledNodes)
                                    {
                                        if (SkillTree.Skillnodes[j].IsMultipleChoiceOption && Tree.ascendancyClasses.GetStartingClass(SkillTree.Skillnodes[i].Name) == Tree.ascendancyClasses.GetStartingClass(SkillTree.Skillnodes[j].Name))
                                        {
                                            Tree.SkilledNodes.Remove(j);
                                            break;
                                        }
                                    }
                                }
                                else if (temp.IsAscendancyStart)
                                {
                                    HashSet<ushort> remove = new HashSet<ushort>(Tree.SkilledNodes.Where(x => SkillTree.Skillnodes[x].ascendancyName == null ? false : SkillTree.Skillnodes[x].ascendancyName != temp.ascendancyName));
                                    foreach (var n in remove)
                                        Tree.SkilledNodes.Remove(n);
                                }
                                Tree.SkilledNodes.Add(i);
                            }

                            Tree.UpdateAvailNodes();

                            _toRemove = Tree.ForceRefundNodePreview(node.Id);
                            if (_toRemove != null)
                                Tree.DrawRefundPreview(_toRemove);
                        }
                    }
                }
            }
            else if ((Tree.ascedancyButtonPos - v).Length < 150)
            {
                Tree.DrawAscendancyButton("Pressed");
                Tree.ToggleAscendancyTree();
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
            UpdateUI();
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
            SkillNode node = null;

            IEnumerable<KeyValuePair<ushort, SkillNode>> nodes =
                SkillTree.Skillnodes.Where(n => ((n.Value.Position - v).Length < 50)).ToList();
            if (nodes.Count() != 0)
            {
                var dnode = nodes.First();
                node = nodes.Where(x => x.Value.ascendancyName != null).DefaultIfEmpty(dnode).First().Value;
            }


            _hoveredNode = node;
            if (node != null && !SkillTree.rootNodeList.Contains(node.Id))
            {
                if (node.ascendancyName != null && !Tree.drawAscendancy)
                    return;
                string className = CharacterNames.NameToContent.Where(x => x.Key == SkillTree.CharName[Tree.Chartype]).First().Value;
                
                if (!Tree.drawAscendancy && node.ascendancyName != null)
                    return;
                else if (!_persistentData.Options.ShowAllAscendancyClasses && node.ascendancyName != null && node.ascendancyName != Tree.ascendancyClasses.GetClassName(className, Tree.AscType))
                    return;
                if (node.IsJewelSocket)
                {
                    Tree.DrawJewelHighlight(node);
                }
                
                if (Tree.SkilledNodes.Contains(node.Id))
                {
                    _toRemove = Tree.ForceRefundNodePreview(node.Id);
                    if (_toRemove != null)
                        Tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = Tree.GetShortestPathTo(node.Id, Tree.SkilledNodes);
                    if (!node.IsMastery)
                        Tree.DrawPath(_prePath);
                }
                var tooltip = node.Name;
                if (node.Attributes.Count != 0)
                    tooltip += "\n" + node.attributes.Aggregate((s1, s2) => s1 + "\n" + s2);
                if (!(_sToolTip.IsOpen && _lasttooltip == tooltip))
                {
                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock
                    {
                        Text = tooltip
                    });
                    if(node.reminderText != null)
                    {
                        sp.Children.Add(new Separator());
                        sp.Children.Add(new TextBlock { Text = node.reminderText.Aggregate((s1, s2) => s1 + '\n' + s2) });
                    }
                    if (_prePath != null && !node.IsMastery)
                    {
                        var points = _prePath.Count;
                        if(_prePath.Any(x => SkillTree.Skillnodes[x].IsAscendancyStart))
                            points--;
                        sp.Children.Add(new Separator());
                        sp.Children.Add(new TextBlock { Text = "Points to skill node: " + points });
                    }

                    _sToolTip.Content = sp;
                    if (!HighlightByHoverKeys.Any(Keyboard.IsKeyDown))
                    {
                        _sToolTip.IsOpen = true;
                    }
                    _lasttooltip = tooltip;
                }
            }
            else if ((Tree.ascedancyButtonPos - v).Length < 150)
            {
                Tree.DrawAscendancyButton("Highlight");
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
                    Tree.ClearJewelHighlight();
                    Tree.DrawAscendancyButton();
                }
            }
        }

        private void zbSkillTreeBackground_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //zbSkillTreeBackground.Child.RaiseEvent(e);
        }

        private void HighlightNodesByHover()
        {
            if (Tree == null)
            {
                return;
            }

            if (_hoveredNode == null || _hoveredNode.Attributes.Count == 0 ||
                !HighlightByHoverKeys.Any(Keyboard.IsKeyDown))
            {
                if (_hoveredNode != null && _hoveredNode.Attributes.Count > 0)
                {
                    _sToolTip.IsOpen = true;
                }

                Tree.HighlightNodesBySearch("", true, NodeHighlighter.HighlightState.FromHover);

                _lastHoveredNode = null;
            }
            else
            {
                _sToolTip.IsOpen = false;

                if (_lastHoveredNode == _hoveredNode)
                {
                    // Not necessary, but stops it from continuously searching when holding down shift.
                    return;
                }

                var search = _hoveredNode.Attributes.Aggregate("^(", (current, attr) => current + (attr.Key + "|"));
                search = search.Substring(0, search.Length - 1);
                search += ")$";
                search = Regex.Replace(search, @"(\+|\-|\%)", @"\$1");
                search = Regex.Replace(search, @"\#", @"[0-9]*\.?[0-9]+");

                Tree.HighlightNodesBySearch(search, true, NodeHighlighter.HighlightState.FromHover,
                    _hoveredNode.Attributes.Count); // Remove last parameter to highlight nodes with any of the attributes.

                _lastHoveredNode = _hoveredNode;
            }
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
                    ItemAttributes = new ItemAttributes(itemData);
                    mnuClearItems.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    _persistentData.CurrentBuild.ItemData = "";
                    ItemAttributes = new ItemAttributes();
                    ClearCurrentItemData();
                    Popup.Error(L10n.Message("An error occurred while attempting to load item data."), ex.Message);
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
            ItemAttributes = new ItemAttributes();
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
            var filterText = tbSavedBuildFilter.Text;

            foreach (PoEBuild item in lvSavedBuilds.Items)
            {
                item.Visible = (className.Equals("All", StringComparison.InvariantCultureIgnoreCase) ||
                                item.Class.Equals(className, StringComparison.InvariantCultureIgnoreCase)) &&
                               (item.Name.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
                                item.Note.Contains(filterText, StringComparison.InvariantCultureIgnoreCase));
            }

            lvSavedBuilds.Items.Refresh();
        }

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
                if(selectedBuild.CurrentlyOpen)
                    SetTitle(selectedBuild.Name);

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

        private void btnSaveBuild_Click(object sender, RoutedEventArgs e)
        {
            SaveBuild();
        }

        private void btnSaveNewBuild_Click(object sender, RoutedEventArgs e)
        {
            SaveNewBuild();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSavedBuilds.SelectedItems.Count <= 0) return;

            if(((PoEBuild)lvSavedBuilds.SelectedItem).CurrentlyOpen)
                NewBuild();
            lvSavedBuilds.Items.Remove(lvSavedBuilds.SelectedItem);
            SaveBuildsToFile();
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
            foreach (PoEBuild item in lvSavedBuilds.Items)
            {
                item.CurrentlyOpen = false;
            }
            build.CurrentlyOpen = true;
            lvSavedBuilds.Items.Refresh();
            SetTitle(build.Name);

            _persistentData.CurrentBuild = PoEBuild.Copy(build);

            tbSkillURL.Text = build.Url;
            SetLevelFromString(build.Level);
            LoadItemData(build.ItemData);
            SetCustomGroups(build.CustomGroups);
        }

        private void NewBuild()
        {
            SetCurrentBuild(new PoEBuild
            {
                Name = "New Build",
                Url = SkillTree.TreeAddress + SkillTree.GetCharacterURL(3),
                Level = "1"
            });
            LoadBuildFromUrl();
        }

        private void SaveBuild()
        {
            var currentOpenBuild =
                (from PoEBuild build in lvSavedBuilds.Items
                 where build.CurrentlyOpen
                 select build).FirstOrDefault();
            if (currentOpenBuild != null)
            {
                currentOpenBuild.Class = cbCharType.Text;
                currentOpenBuild.CharacterName = _persistentData.CurrentBuild.CharacterName;
                currentOpenBuild.AccountName = _persistentData.CurrentBuild.AccountName;
                currentOpenBuild.Level = GetLevelAsString();
                currentOpenBuild.PointsUsed = NormalUsedPoints.Content.ToString();
                currentOpenBuild.Url = tbSkillURL.Text;
                currentOpenBuild.ItemData = _persistentData.CurrentBuild.ItemData;
                currentOpenBuild.LastUpdated = DateTime.Now;
                currentOpenBuild.CustomGroups = _attributeGroups.CopyCustomGroups();
                SetCurrentBuild(currentOpenBuild);
                SaveBuildsToFile();
            }
            else
            {
                SaveNewBuild();
            }
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
                    Level = GetLevelAsString(),
                    Class = cbCharType.Text,
                    PointsUsed = NormalUsedPoints.Content.ToString(),
                    Url = tbSkillURL.Text,
                    Note = formBuildName.GetNote(),
                    CharacterName = formBuildName.GetCharacterName(),
                    AccountName = formBuildName.GetAccountName(),
                    ItemData = formBuildName.GetItemData(),
                    LastUpdated = DateTime.Now,
                    CustomGroups = _attributeGroups.CopyCustomGroups()
                };
                SetCurrentBuild(newBuild);
                lvSavedBuilds.Items.Add(newBuild);
            }

            if (lvSavedBuilds.Items.Count > 0)
            {
                SaveBuildsToFile();
            }
            lvSavedBuilds.SelectedIndex = lvSavedBuilds.Items.Count - 1;
            if(lvSavedBuilds.SelectedIndex != -1)
                lvSavedBuilds.ScrollIntoView(lvSavedBuilds.Items[lvSavedBuilds.Items.Count - 1]);
        }

        private void SaveBuildsToFile()
        {
            try
            {
                _persistentData.SetBuilds(lvSavedBuilds.Items);
                _persistentData.SavePersistentDataToFile();
            }
            catch (Exception e)
            {
                Popup.Error(L10n.Message("An error occurred during a save operation."), e.Message);
            }
        }

        private async void LoadBuildFromUrl()
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(Tree, tbSkillURL.Text);
                    tbSkillURL.Text = Tree.SaveToURL();
                }
                else if (tbSkillURL.Text.Contains("google.com"))
                {
                    Match match = Regex.Match(tbSkillURL.Text, @"q=(.*?)&");
                    if (match.Success)
                    {
                        tbSkillURL.Text = match.ToString().Replace("q=", "").Replace("&", "");
                        LoadBuildFromUrl();
                    }
                    else
                        throw new Exception("The URL you are trying to load is invalid.");
                }
                else if (tbSkillURL.Text.Contains("tinyurl.com") || tbSkillURL.Text.Contains("poeurl.com"))
                {
                    var skillUrl = tbSkillURL.Text.Replace("preview.", "");
                    if (skillUrl.Contains("poeurl.com") && !skillUrl.Contains("redirect.php"))
                    {
                        skillUrl = skillUrl.Replace("http://poeurl.com/",
                            "http://poeurl.com/redirect.php?url=");
                    }

                    var response =
                        await AwaitAsyncTask(L10n.Message("Resolving shortened tree address"),
                            new HttpClient().GetAsync(skillUrl, HttpCompletionOption.ResponseHeadersRead));
                    response.EnsureSuccessStatusCode();
                    if (Regex.IsMatch(response.RequestMessage.RequestUri.ToString(), SkillTree.TreeRegex))
                        tbSkillURL.Text = response.RequestMessage.RequestUri.ToString();
                    else
                        throw new Exception("The URL you are trying to load is invalid.");
                    LoadBuildFromUrl();
                }
                else
                {
                    if (tbSkillURL.Text.Contains("characterName") || tbSkillURL.Text.Contains("accountName"))
                        tbSkillURL.Text = Regex.Replace(tbSkillURL.Text, @"\?.*", "");
                    tbSkillURL.Text = Regex.Replace(tbSkillURL.Text, SkillTree.TreeRegex, SkillTree.TreeAddress);
                    Tree.LoadFromURL(tbSkillURL.Text);
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
            catch (Exception ex)
            {
                tbSkillURL.Text = Tree.SaveToURL();
                Popup.Error(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
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
            var listViewItem = ((DependencyObject)e.OriginalSource).FindAnchestor<ListViewItem>();

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
                ListViewItem listViewItem = ((DependencyObject)e.OriginalSource).FindAnchestor<ListViewItem>();

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
            Tree.HighlightNodesBySearch(tbSearch.Text, cbRegEx.IsChecked != null && cbRegEx.IsChecked.Value, NodeHighlighter.HighlightState.FromSearch);
        }

        private void ClearSearch()
        {
            tbSearch.Text = "";
            SearchUpdate();
        }

        private void tbSkillURL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && NoAsyncTaskRunning)
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

        private async void StartDownloadPoeUrl()
        {
            var regx =
                new Regex(
                    "https?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?",
                    RegexOptions.IgnoreCase);

            var matches = regx.Matches(tbSkillURL.Text);

            if (matches.Count == 1)
            {
                try
                {
                    var url = matches[0].ToString();
                    if (!url.ToLower().StartsWith(SkillTree.TreeAddress))
                    {
                        return;
                    }
                    // PoEUrl can't handle https atm.
                    url = url.Replace("https://", "http://");

                    var result =
                        await AwaitAsyncTask(L10n.Message("Generating PoEUrl of Skill tree"),
                            new HttpClient().GetStringAsync("http://poeurl.com/shrink.php?url=" + url));
                    ShowPoeUrlMessageAndAddToClipboard("http://poeurl.com/" + result.Trim());
                }
                catch (Exception ex)
                {
                    Popup.Error(L10n.Message("An error occurred while attempting to contact the PoEUrl location."), ex.Message);
                }
            }
        }

        private void ShowPoeUrlMessageAndAddToClipboard(string poeurl)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
                Popup.Info(L10n.Message("The PoEUrl link has been copied to Clipboard.") + "\n\n" + poeurl);
            }
            catch (Exception ex)
            {
                Popup.Error(L10n.Message("An error occurred while copying to Clipboard."), ex.Message);
            }
        }

        #endregion

        #region Theme

        private void mnuSetTheme_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            SetTheme(menuItem.Tag as string);
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

            SetAccent(menuItem.Tag as string);
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

        #region Legacy

        /// <summary>
        /// Compares the AssemblyVersion against the one in PersistentData and makes
        /// nessary updates when versions don't match.
        /// </summary>
        private void CheckAppVersionAndDoNecessaryChanges()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var productVersion = fvi.ProductVersion;
            var persistentDataVersion = _persistentData.AppVersion;
            if (productVersion == persistentDataVersion)
                return;
            if(string.IsNullOrEmpty(persistentDataVersion))
                ImportLegacySavedBuilds();
            if (String.CompareOrdinal("2.2.4", persistentDataVersion) > 0)
                SetCurrentOpenBuildBasedOnName();

            _persistentData.AppVersion = productVersion;
        }

        private void SetCurrentOpenBuildBasedOnName()
        {
            var buildNameMatch =
                (from PoEBuild build in lvSavedBuilds.Items
                    where build.Name == _persistentData.CurrentBuild.Name
                    select build).FirstOrDefault();
            if (buildNameMatch != null)
            {
                foreach (PoEBuild item in lvSavedBuilds.Items)
                {
                    item.CurrentlyOpen = false;
                }
                buildNameMatch.CurrentlyOpen = true;
                lvSavedBuilds.Items.Refresh();
            }
        }

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
                Popup.Error(L10n.Message("An error occurred while attempting to load saved builds."), ex.Message);
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
                int atype;
                SkillTree.DecodeURL(build.Url, out nodes, out ctype, out atype);

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

            Tree.DrawTreeComparisonHighlight();
            UpdateUI();
        }

        private void ToggleTreeComparison_Click(object sender, RoutedEventArgs e)
        {
            lvSavedBuilds_SelectionChanged(null, null);
        }

        private void Menu_RedownloadItemAssets(object sender, RoutedEventArgs e)
        {
            string sMessageBoxText = L10n.Message("The existing Skill Item assets will be deleted and new assets will be downloaded.")
                       + "\n\n" + L10n.Message("Do you want to continue?");

            var rsltMessageBox = Popup.Ask(sMessageBoxText, MessageBoxImage.Warning);

            string appDataPath = AppData.GetFolder(true);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    if (Directory.Exists(Path.Combine(appDataPath, "Data")))
                    {
                        try
                        {
                            if (Directory.Exists(Path.Combine(appDataPath, "DataBackup")))
                                Directory.Delete("DataBackup", true);
                            Directory.Move(Path.Combine(appDataPath, "Data"), Path.Combine(appDataPath, "DataBackup"));


                            var bases = new List<ItemBase>();
                            var images = new List<Tuple<string, string>>();

                            StartLoadingWindow(L10n.Message("Downloading Item assets"));
                            UpdateLoadingWindow(0, 3);
                            ItemAssetDownloader.ExtractJewelry(bases, images);
                            UpdateLoadingWindow(1, 3);
                            ItemAssetDownloader.ExtractArmors(bases, images);
                            UpdateLoadingWindow(2, 3);
                            ItemAssetDownloader.ExtractWeapons(bases, images);
                            UpdateLoadingWindow(3, 3);

                            new System.Xml.Linq.XElement("ItemBaseList", bases.Select(b => b.Serialize())).Save(Path.Combine(AppData.GetFolder(@"Data\Equipment"), "Itemlist.xml"));

                            var imgroups = images.GroupBy(t => t.Item2).ToArray();

                            UpdateLoadingWindow(0, imgroups.Length);

                            var dir = AppData.GetFolder(@"Data\Equipment\Assets");
                            using (var client = new WebClient())
                            {
                                for (int i = 0; i < imgroups.Length; i++)
                                {
                                    using (var ms = new MemoryStream(client.DownloadData(imgroups[i].Key)))
                                    {
                                        PngBitmapDecoder dec = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                                        var image = dec.Frames[0];
                                        var cropped = new CroppedBitmap(image, new Int32Rect(4, 4, image.PixelWidth - 8, image.PixelHeight - 8));
                                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                                        encoder.Frames.Add(BitmapFrame.Create(cropped));

                                        using (var m = new MemoryStream())
                                        {
                                            encoder.Save(m);

                                            foreach (var item in imgroups[i])
                                            {
                                                using (var f = File.Create(Path.Combine(dir, item.Item1 + ".png")))
                                                {
                                                    m.Seek(0, SeekOrigin.Begin);
                                                    m.CopyTo(f);
                                                }

                                            }
                                        }

                                        UpdateLoadingWindow(i + 1, imgroups.Length);
                                    }
                                }
                            }

                            foreach (var file in new DirectoryInfo(Path.Combine(appDataPath, @"DataBackup")).GetFiles())
                                file.CopyTo(Path.Combine(Path.Combine(appDataPath, @"Data"), file.Name));
                            
                            File.Copy(Path.Combine(AppData.GetFolder(@"DataBackup\Equipment"), "Affixlist.xml"), Path.Combine(AppData.GetFolder(@"Data\Equipment"), "Affixlist.xml"));
                            
                            Directory.Move(Path.Combine(appDataPath, @"DataBackup\Assets"), Path.Combine(appDataPath, @"Data\Assets"));
                            Directory.Move(Path.Combine(appDataPath, @"DataBackup\PseudoAttributes"), Path.Combine(appDataPath, @"Data\PseudoAttributes"));
                            if (Directory.Exists(Path.Combine(appDataPath, "DataBackup")))
                                Directory.Delete(Path.Combine(appDataPath, "DataBackup"), true);

                            CloseLoadingWindow();
                        }
                        catch (Exception ex)
                        {
                            if (Directory.Exists(appDataPath + "Data") && Directory.Exists(appDataPath + "DataBackup"))
                                Directory.Delete(Path.Combine(appDataPath, "Data"), true);
                            try
                            {
                                CloseLoadingWindow();
                            }
                            catch (Exception)
                            {
                                //Nothing
                            }
                            if (Directory.Exists(appDataPath + "DataBackup"))
                                Directory.Move(Path.Combine(appDataPath, "DataBackup"), Path.Combine(appDataPath, "Data"));
                            Popup.Error(L10n.Message("Error while downloading assets."));
                        }
                    }
                    break;

                case MessageBoxResult.No:
                    //Do nothing
                    break;
            }
        }

        private void Button_Craft_Click(object sender, RoutedEventArgs e)
        {
            var w = new CraftWindow() { Owner = this };
            if (w.ShowDialog() == true)
            {
                var item = w.Item;
                if (PersistentData.StashItems.Count > 0)
                    item.Y = PersistentData.StashItems.Max(i => i.Y + i.Height);

                Stash.Items.Add(item);

                Stash.AddHighlightRange(new IntRange() { From = item.Y, Range = item.Height });
                Stash.asBar.Value = item.Y;
            }
        }

        private void deleteRect_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ItemVisualizer)) && (e.AllowedEffects & DragDropEffects.Move) != 0)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private void deleteRect_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ItemVisualizer)) && (e.AllowedEffects & DragDropEffects.Move) != 0)
            {
                e.Effects = DragDropEffects.Move;

                var d = e.Data.GetData(typeof(ItemVisualizer)) as ItemVisualizer;

                var st = d.FindAnchestor<Stash>();
                if (st != null)
                {
                    st.RemoveItem(d.Item);
                }
                else
                {
                    d.Item = null;
                }
                deleteRect.Opacity = 0.0;
            }
        }

        private void deleteRect_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ItemVisualizer)) && (e.AllowedEffects & DragDropEffects.Move) != 0)
            {
                deleteRect.Opacity = 0.3;
            }
        }

        private void deleteRect_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ItemVisualizer)) && (e.AllowedEffects & DragDropEffects.Move) != 0)
            {
                deleteRect.Opacity = 0.0;
            }
        }

        private void Menu_ImportStash(object sender, RoutedEventArgs e)
        {
            var diw = new DownloadStashWindow(_persistentData.CurrentBuild.CharacterName, _persistentData.CurrentBuild.League) { Owner = this };
            diw.ShowDialog();
            _persistentData.CurrentBuild.CharacterName = diw.GetCharacterName();
            _persistentData.CurrentBuild.League = diw.GetAccountName();
        }

        #region Async task helpers

        private void AsyncTaskStarted(string infoText)
        {
            NoAsyncTaskRunning = false;
            TitleStatusTextBlock.Text = infoText;
            TitleStatusButton.Visibility = Visibility.Visible;
        }

        private void AsyncTaskCompleted()
        {
            TitleStatusButton.Visibility = Visibility.Hidden;

            NoAsyncTaskRunning = true;
        }

        private async Task<TResult> AwaitAsyncTask<TResult>(string infoText, Task<TResult> task)
        {
            AsyncTaskStarted(infoText);
            try
            {
                return await task;
            }
            finally
            {
                AsyncTaskCompleted();
            }
        }

        private async Task AwaitAsyncTask(string infoText, Task task)
        {
            AsyncTaskStarted(infoText);
            try
            {
                await task;
            }
            finally
            {
                AsyncTaskCompleted();
            }
        }

        #endregion

        public override IWindowPlacementSettings GetWindowPlacementSettings()
        {
            var settings = base.GetWindowPlacementSettings();
            if (WindowPlacementSettings == null)
            {
                // Settings just got created, give them a proper SettingsProvider.
                var appSettings = settings as ApplicationSettingsBase;
                if (appSettings == null)
                {
                    // Nothing we can do here.
                    return settings;
                }
                var provider = new CustomSettingsProvider(appSettings.SettingsKey);
                // This may look ugly, but it is needed and nulls are the only parameter
                // Initialize is ever called with by anything.
                provider.Initialize(null, null);
                appSettings.Providers.Add(provider);
                // Change the provider for each SettingsProperty.
                foreach (var property in appSettings.Properties)
                {
                    ((SettingsProperty) property).Provider = provider;
                }
                appSettings.Reload();
            }
            return settings;
        }
    }
}