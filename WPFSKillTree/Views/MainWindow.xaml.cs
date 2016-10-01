using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro;
using MahApps.Metro.Controls;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls;
using POESKillTree.Controls.Dialogs;
using POESKillTree.ItemFilter.Views;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.ViewModels;
using POESKillTree.Utils;
using POESKillTree.Utils.Converter;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels;
using Attribute = POESKillTree.ViewModels.Attribute;

namespace POESKillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>
        /// The set of keys of which one needs to be pressed to highlight similar nodes on hover.
        /// </summary>
        private static readonly Key[] HighlightByHoverKeys = { Key.LeftShift, Key.RightShift };

        public event PropertyChangedEventHandler PropertyChanged;

        public IPersistentData PersistentData { get; } = App.PersistentData;

        private readonly List<Attribute> _allAttributesList = new List<Attribute>();
        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly List<ListGroupItem> _defenceList = new List<ListGroupItem>();
        private readonly Dictionary<string, AttributeGroup> _defenceListGroups = new Dictionary<string, AttributeGroup>();
        private readonly List<ListGroupItem> _offenceList = new List<ListGroupItem>();
        private readonly Dictionary<string, AttributeGroup> _offenceListGroups = new Dictionary<string, AttributeGroup>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
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
                if (value == _itemAttributes)
                    return;
                _itemAttributes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemAttributes"));
            }
        }

        private SkillTree _tree;
        public SkillTree Tree
        {
            get { return _tree; }
            private set
            {
                if (_tree != null)
                    _tree.PropertyChanged -= Tree_PropertyChanged;
                _tree = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tree"));
            }
        }
        private async Task<SkillTree> CreateSkillTreeAsync(ProgressDialogController controller,
            AssetLoader assetLoader = null)
        {
            var tree = await SkillTree.CreateAsync(PersistentData, DialogCoordinator.Instance, controller, assetLoader);
            DialogParticipation.SetRegister(this, tree);
            tree.PropertyChanged += Tree_PropertyChanged;
            if (BuildsControlViewModel != null)
                BuildsControlViewModel.SkillTree = tree;
            if (TreeGeneratorInteraction != null)
                TreeGeneratorInteraction.SkillTree = tree;
            return tree;
        }

        private BuildsControlViewModel _buildsControlViewModel;
        public BuildsControlViewModel BuildsControlViewModel
        {
            get { return _buildsControlViewModel; }
            private set
            {
                _buildsControlViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildsControlViewModel)));
            }
        }

        private Vector2D _addtransform;
        private bool _justLoaded;
        private string _lasttooltip;

        private Vector2D _multransform;

        private List<SkillNode> _prePath;
        private HashSet<SkillNode> _toRemove;

        private readonly Stack<string> _undoList = new Stack<string>();
        private readonly Stack<string> _redoList = new Stack<string>();

        private MouseButton _lastMouseButton;
        private bool _userInteraction = false;
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NoAsyncTaskRunning"));
            }
        }

        private TreeGeneratorInteraction _treeGeneratorInteraction;

        public TreeGeneratorInteraction TreeGeneratorInteraction
        {
            get { return _treeGeneratorInteraction; }
            private set
            {
                _treeGeneratorInteraction = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TreeGeneratorInteraction)));
            }
        }

        public string MainWindowTitle { get; } =
            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductName;

        /// <summary>
        /// Set to true when CurrentBuild.TreeUrl was set after direct SkillTree changes so the SkillTree
        /// doesn't need to be reloaded.
        /// </summary>
        private bool _skipLoadOnCurrentBuildTreeChange;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RegisterPersistentDataHandlers()
        {
            // Register handlers
            PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
            PersistentData.CurrentBuild.Bandits.PropertyChanged += (o, a) => UpdateUI();
            // Re-register handlers when PersistentData.CurrentBuild is set.
            PersistentData.PropertyChanged += async (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PersistentData.CurrentBuild):
                        PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
                        PersistentData.CurrentBuild.Bandits.PropertyChanged += (o, a) => UpdateUI();
                        await CurrentBuildChanged();
                        break;
                    case nameof(PersistentData.SelectedBuild):
                        UpdateTreeComparision();
                        break;
                }
            };
            // This makes sure CurrentBuildOnPropertyChanged is called only
            // on the PoEBuild instance currently stored in PersistentData.CurrentBuild.
            PersistentData.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == nameof(PersistentData.CurrentBuild))
                {
                    TreeGeneratorInteraction?.SaveSettings();
                    PersistentData.CurrentBuild.PropertyChanged -= CurrentBuildOnPropertyChanged;
                }
            };
            
        }

        private async void CurrentBuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(PoEBuild.ItemData):
                    await LoadItemData();
                    break;
                case nameof(PoEBuild.TreeUrl):
                    if (!_skipLoadOnCurrentBuildTreeChange)
                        await LoadBuildFromUrlAsync(PersistentData.CurrentBuild.TreeUrl);
                    break;
                case nameof(PoEBuild.CheckedNodeIds):
                case nameof(PoEBuild.CrossedNodeIds):
                    Tree.ResetTaggedNodes();
                    break;
                case nameof(PoEBuild.AdditionalData):
                    TreeGeneratorInteraction?.LoadSettings();
                    break;
            }
        }

        //This whole region, along with most of GroupStringConverter, makes up our user-defined attribute group functionality - Sectoidfodder 02/29/16
        #region Attribute grouping helpers

        //there's probably a better way that doesn't break if tab ordering changes but I'm UI-challenged
        private ListBox GetActiveAttributeGroupList()
        {
            if (tabControl1.SelectedIndex == 2)
                return lbAllAttr;
            else if (tabControl1.SelectedIndex == 0)
                return lbAttr;
            else
                return null;
        }

        //Necessary to update the summed numbers in group names before every refresh
        private void RefreshAttributeLists()
        {
            _attributeGroups.UpdateGroupNames(Equals(GetActiveAttributeGroupList(), lbAllAttr)
                ? _allAttributesList
                : _attiblist);
            _attributeCollection.Refresh();
            _allAttributeCollection.Refresh();
        }

        private void SetCustomGroups(IList<string[]> customgroups)
        {
            cmAddToGroup.Items.Clear();
            cmDeleteGroup.Items.Clear();

            var groupnames = new List<string>();

            foreach (var gp in customgroups)
            {
                if (!groupnames.Contains(gp[1]))
                {
                    groupnames.Add(gp[1]);
                }
            }

            cmAddToGroup.IsEnabled = false;
            cmDeleteGroup.IsEnabled = false;

            foreach (var name in groupnames)
            {
                var newSubMenu = new MenuItem {Header = name};
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem {Header = name};
                newSubMenu.Click += DeleteGroup;
                cmDeleteGroup.Items.Add(newSubMenu);
                cmDeleteGroup.IsEnabled = true;
            }

            _attributeGroups.ResetGroups(customgroups);
            RefreshAttributeLists();
        }

        //Adds currently selected attributes to a new group
        private async void CreateGroup(object sender, RoutedEventArgs e)
        {
            var lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            var attributelist = new List<string>();
            foreach (var o in lb.SelectedItems)
            {
                attributelist.Add(o.ToString());
            }

            //Build and show form to enter group name
            var name = await this.ShowInputAsync(L10n.Message("Create New Attribute Group"), L10n.Message("Group name"));
            if (!string.IsNullOrEmpty(name))
            {
                if (_attributeGroups.AttributeGroups.ContainsKey(name))
                {
                    await this.ShowInfoAsync(L10n.Message("A group with that name already exists."));
                    return;
                }

                //Add submenus that add to and delete the new group
                var newSubMenu = new MenuItem {Header = name};
                newSubMenu.Click += AddToGroup;
                cmAddToGroup.Items.Add(newSubMenu);
                cmAddToGroup.IsEnabled = true;
                newSubMenu = new MenuItem {Header = name};
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
            var lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            var attributelist = new List<string>();
            foreach (var o in lb.SelectedItems)
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
            var lb = GetActiveAttributeGroupList();
            if (lb == null)
                return;
            var attributelist = new List<string>();
            foreach (var o in lb.SelectedItems)
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
            for (var i = 0; i < cmAddToGroup.Items.Count; i++)
            {
                if (((MenuItem)cmAddToGroup.Items[i]).Header.ToString().ToLower().Equals(((MenuItem)sender).Header.ToString().ToLower()))
                {
                    cmAddToGroup.Items.RemoveAt(i);
                    if (cmAddToGroup.Items.Count == 0)
                        cmAddToGroup.IsEnabled = false;
                    break;
                }
            }
            for (var i = 0; i < cmDeleteGroup.Items.Count; i++)
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var controller = await this.ShowProgressAsync(L10n.Message("Initialization"),
                        L10n.Message("Initalizing window ..."));
            controller.Maximum = 1;
            controller.SetIndeterminate();

            var itemDBTask = Task.Run(() =>
            {
                const string itemDBPrefix = "Data/ItemDB/";
                Directory.CreateDirectory(AppData.GetFolder(itemDBPrefix));
                // First file instantiates the ItemDB.
                ItemDB.Load(itemDBPrefix + "GemList.xml");
                // Merge all other files from the ItemDB path.
                Directory.GetFiles(AppData.GetFolder(itemDBPrefix))
                    .Select(Path.GetFileName)
                    .Where(f => f != "GemList.xml")
                    .Select(f => itemDBPrefix + f)
                    .ForEach(ItemDB.Merge);
                // Merge the user specified things.
                ItemDB.Merge("ItemsLocal.xml");
                ItemDB.Index();
            });
            // We can use this now as setting the context for this to the SkillTree instance happens later
            DialogParticipation.SetRegister(this, PersistentData);
            var persistentDataTask = PersistentData.InitializeAsync(DialogCoordinator.Instance);
            await itemDBTask;
            await persistentDataTask;
            RegisterPersistentDataHandlers();

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
            cmCreateGroup = new MenuItem {Header = "Create new group"};
            cmCreateGroup.Click += CreateGroup;
            cmAddToGroup = new MenuItem
            {
                Header = "Add to group...",
                IsEnabled = false
            };
            cmDeleteGroup = new MenuItem
            {
                Header = "Delete group...",
                IsEnabled = false
            };
            cmRemoveFromGroup = new MenuItem {Header = "Remove from group"};
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
            _attributeCollection.GroupDescriptions?.Add(new PropertyGroupDescription("Text", _attributeGroups));
            _attributeCollection.CustomSort = _attributeGroups;
            lbAttr.ItemsSource = _attributeCollection;
            lbAttr.SelectionMode = SelectionMode.Extended;
            lbAttr.ContextMenu = _attributeContextMenu;

            _allAttributeCollection = new ListCollectionView(_allAttributesList);
            _allAttributeCollection.GroupDescriptions?.Add(new PropertyGroupDescription("Text", _attributeGroups));
            _allAttributeCollection.CustomSort = _attributeGroups;
            lbAllAttr.ItemsSource = _allAttributeCollection;
            lbAllAttr.SelectionMode = SelectionMode.Extended;
            lbAllAttr.ContextMenu = _attributeContextMenu;

            _defenceCollection = new ListCollectionView(_defenceList);
            _defenceCollection.GroupDescriptions?.Add(new PropertyGroupDescription("Group"));
            listBoxDefence.ItemsSource = _defenceCollection;

            _offenceCollection = new ListCollectionView(_offenceList);
            _offenceCollection.GroupDescriptions?.Add(new PropertyGroupDescription("Group"));
            listBoxOffence.ItemsSource = _offenceCollection;

            cbCharType.ItemsSource =
                CharacterNames.NameToContent.Select(
                    x => new ComboBoxItem {Name = x.Key, Content = x.Value});
            cbAscType.SelectedIndex = 0;

            Stash.Bookmarks = PersistentData.StashBookmarks;

            // Set theme & accent.
            SetTheme(PersistentData.Options.Theme);
            SetAccent(PersistentData.Options.Accent);

            controller.SetMessage(L10n.Message("Loading skill tree assets ..."));
            Tree = await CreateSkillTreeAsync(controller);
            await Task.Delay(1); // Give the progress dialog a chance to update
            recSkillTree.Width = SkillTree.SkillTreeRect.Width / SkillTree.SkillTreeRect.Height * recSkillTree.Height;
            recSkillTree.UpdateLayout();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

            _multransform = SkillTree.SkillTreeRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = SkillTree.SkillTreeRect.TopLeft;

            controller.SetMessage(L10n.Message("Initalizing window ..."));
            controller.SetIndeterminate();
            await Task.Delay(1); // Give the progress dialog a chance to update

            _justLoaded = true;

            // loading last build
            await CurrentBuildChanged();

            _justLoaded = false;
            // loading saved build
            PersistentData.Options.PropertyChanged += Options_PropertyChanged;
            PopulateAsendancySelectionList();
            BuildsControlViewModel = new BuildsControlViewModel(ExtendedDialogCoordinator.Instance, PersistentData, Tree);
            UpdateTreeComparision();
            TreeGeneratorInteraction =
                new TreeGeneratorInteraction(SettingsDialogCoordinator.Instance, PersistentData, Tree);
            TreeGeneratorInteraction.RunFinished += (o, args) =>
            {
                UpdateUI();
                SetCurrentBuildUrlFromTree();
            };

            await controller.CloseAsync();
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Options.ShowAllAscendancyClasses):
                    Tree.ToggleAscendancyTree(PersistentData.Options.ShowAllAscendancyClasses);
                    break;
                case nameof(Options.TreeComparisonEnabled):
                    UpdateTreeComparision();
                    break;
            }
            SearchUpdate();
        }

        private void Tree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SkillTree.Level):
                    PersistentData.CurrentBuild.Level = Tree.Level;
                    break;
                case nameof(SkillTree.Chartype):
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAsendancySelectionList();
                    break;
            }
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
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 0;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D2:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 1;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D3:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 2;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D4:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 3;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D5:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 4;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D6:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 5;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.D7:
                        _userInteraction = true;
                        cbCharType.SelectedIndex = 6;
                        cbAscType.SelectedIndex = 0;
                        break;
                    case Key.OemPlus:
                    case Key.Add:
                        zbSkillTreeBackground.ZoomIn(Mouse.PrimaryDevice);
                        break;
                    case Key.OemMinus:
                    case Key.Subtract:
                        zbSkillTreeBackground.ZoomOut(Mouse.PrimaryDevice);
                        break;
                    case Key.Z:
                        tbSkillURL_Undo();
                        break;
                    case Key.Y:
                        tbSkillURL_Redo();
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

        private bool? _canClose;

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_canClose.HasValue)
            {
                // We want to close later
                e.Cancel = true;
                // Stop close calls while async processing from closing
                _canClose = false;
                var message = L10n.Message("There are unsaved builds. Do you want to save them before closing?\n\n"
                                           + "Canceling stops the program from closing and does not save any builds.");
                // Might affect unsaved builds state, so needs to be done here.
                TreeGeneratorInteraction?.SaveSettings();
                if (await BuildsControlViewModel.HandleUnsavedBuilds(message))
                {
                    // User wants to close
                    _canClose = true;
                    // Calling Close() here again is not possible as the Closing event might still be handled
                    // (Close() is not allowed while a previous one is not completely processed)
                    Application.Current.Shutdown();
                }
                else
                {
                    // User doesn't want to close. Reset _canClose.
                    _canClose = null;
                }
            }
            else if (!_canClose.Value)
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Menu

        private async void Menu_UntagAllNodes(object sender, RoutedEventArgs e)
        {
            var response = await this.ShowQuestionAsync(L10n.Message("Are you sure?"),
                title: L10n.Message("Untag All Skill Nodes"), image: MessageBoxImage.None);
            if (response == MessageBoxResult.Yes)
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

        private async void Menu_ScreenShot(object sender, RoutedEventArgs e)
        {
            const int maxsize = 3000;
            Rect2D contentBounds = Tree.ActivePaths.ContentBounds;
            contentBounds *= 1.2;
            if (!double.IsNaN(contentBounds.Width) && !double.IsNaN(contentBounds.Height))
            {
                var aspect = contentBounds.Width / contentBounds.Height;
                var xmax = contentBounds.Width;
                var ymax = contentBounds.Height;
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
                var db = new VisualBrush(Tree.SkillTreeVisual)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = contentBounds
                };
                var dw = new DrawingVisual();

                using (var dc = dw.RenderOpen())
                {
                    dc.DrawRectangle(db, null, new Rect(0, 0, xmax, ymax));
                }
                _clipboardBmp.Render(dw);
                _clipboardBmp.Freeze();

                //Save image in clipboard
                Clipboard.SetImage(_clipboardBmp);

                //Convert renderTargetBitmap to bitmap
                var stream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_clipboardBmp));
                encoder.Save(stream);

                var image = System.Drawing.Image.FromStream(stream);

                // Configure save file dialog box
                var dialog = new Microsoft.Win32.SaveFileDialog();

                // Default file name -- current build name ("buildname - xxx points used")
                var skilledNodes = (uint) Tree.GetPointCount()["NormalUsed"];
                dialog.FileName = PersistentData.CurrentBuild.Name + " - " + string.Format(L10n.Plural("{0} point", "{0} points", skilledNodes), skilledNodes);

                dialog.DefaultExt = ".jpg"; // Default file extension
                dialog.Filter = "JPEG (*.jpg, *.jpeg)|*.jpg;|PNG (*.png)|*.png"; // Filter files by extension
                dialog.OverwritePrompt = true;

                // Show save file dialog box
                var result = dialog.ShowDialog();

                // Continue if the user did select a path
                if (result.HasValue && result == true)
                {
                    System.Drawing.Imaging.ImageFormat format;
                    var fileExtension = Path.GetExtension(dialog.FileName);

                    //set the selected data type
                    switch (fileExtension)
                    {
                        case ".png":
                            format = System.Drawing.Imaging.ImageFormat.Png;
                            break;
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
                await this.ShowInfoAsync(L10n.Message("Your build must use at least one node to generate a screenshot"), title: "Screenshot Generator");
            }
        }

        private async void Menu_ImportItems(object sender, RoutedEventArgs e)
        {
            await this.ShowDialogAsync(
                new DownloadItemsViewModel(PersistentData.CurrentBuild),
                new DownloadItemsWindow());
        }

        private async void Menu_ImportStash(object sender, RoutedEventArgs e)
        {
            var vm = new DownloadStashViewModel(DialogCoordinator.Instance, PersistentData, Stash);
            await this.ShowDialogAsync(vm, new DownloadStashWindow(), () => vm.ViewLoaded());
        }

        private void Menu_ItemFilterEditor(object sender, RoutedEventArgs e)
        {
            var window = new ItemFilterOpenWindow() { Owner = this };
            window.ShowDialog();
        }

        private async void Menu_CopyStats(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var at in _attiblist)
            {
                sb.AppendLine(at.ToString());
            }
            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred while copying to Clipboard."), ex.Message);
            }
        }

        private async void Menu_RedownloadTreeAssets(object sender, RoutedEventArgs e)
        {
            var sMessageBoxText = L10n.Message("The existing Skill tree assets will be deleted and new assets will be downloaded.")
                                     + "\n\n" + L10n.Message("Do you want to continue?");

            var rsltMessageBox = await this.ShowQuestionAsync(sMessageBoxText, image: MessageBoxImage.Warning);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    var controller = await this.ShowProgressAsync(L10n.Message("Downloading skill tree assets ..."), null);
                    controller.Maximum = 1;
                    controller.SetProgress(0);
                    var assetLoader = new AssetLoader(new HttpClient(), AppData.GetFolder("Data", true), false);
                    try
                    {
                        assetLoader.MoveToBackup();

                        SkillTree.ClearAssets(); //enable recaching of assets
                        Tree = await CreateSkillTreeAsync(controller, assetLoader); //create new skilltree to reinitialize cache
                        recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

                        await LoadBuildFromUrlAsync();
                        _justLoaded = false;

                        assetLoader.DeleteBackup();
                    }
                    catch (Exception ex)
                    {
                        assetLoader.RestoreBackup();
                        await this.ShowErrorAsync(L10n.Message("An error occurred while downloading assets."), ex.Message);
                    }
                    await controller.CloseAsync();
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

        private async void Menu_OpenHelp(object sender, RoutedEventArgs e)
        {
            await this.ShowDialogAsync(new CloseableViewModel(), new HelpWindow());
        }

        private async void Menu_OpenSettings(object sender, RoutedEventArgs e)
        {
            await this.ShowDialogAsync(
                new SettingsMenuViewModel(PersistentData, DialogCoordinator.Instance, BuildsControlViewModel),
                new SettingsMenuWindow());
        }

        private async void Menu_OpenHotkeys(object sender, RoutedEventArgs e)
        {
            await this.ShowDialogAsync(new CloseableViewModel(), new HotkeysWindow());
        }

        private async void Menu_OpenAbout(object sender, RoutedEventArgs e)
        {
            await this.ShowDialogAsync(new CloseableViewModel(), new AboutWindow());
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
                    await this.ShowInfoAsync(L10n.Message("You have the latest version!"));
                }
                else
                {
                    var message = release.IsUpdate
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

                    var download = await this.ShowQuestionAsync(message, title: L10n.Message("Continue installation?"),
                        image: release.IsPrerelease ? MessageBoxImage.Warning : MessageBoxImage.Question);
                    if (download == MessageBoxResult.Yes)
                        await InstallUpdateAsync();
                    else
                        Updater.Dispose();
                }
            }
            catch (UpdaterException ex)
            {
                await this.ShowErrorAsync(
                    L10n.Message("An error occurred while attempting to contact the update location."),
                    ex.Message);
            }
        }

        // Starts update process.
        private async Task InstallUpdateAsync()
        {
            var controller = await this.ShowProgressAsync(L10n.Message("Downloading latest version"), null, true);
            controller.Maximum = 100;
            controller.Canceled += (sender, args) => Updater.Cancel();
            try
            {
                var downloadCs = new TaskCompletionSource<AsyncCompletedEventArgs>();
                Updater.Download((sender, args) => downloadCs.SetResult(args),
                    (sender, args) => controller.SetProgress(args.ProgressPercentage));

                var result = await downloadCs.Task;
                await controller.CloseAsync();
                await UpdateDownloadCompleted(result);
            }
            catch (UpdaterException ex)
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred during the download operation."),
                    ex.Message);
                await controller.CloseAsync();
            }
        }

        // Invoked when update download completes, aborts or fails.
        private async Task UpdateDownloadCompleted(AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) // Check whether download was cancelled.
            {
                Updater.Dispose();
            }
            else if (e.Error != null) // Check whether error occurred.
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred during the download operation."), e.Error.Message);
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
                    await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to start the installation."), ex.Message);
                }
            }
        }

#endregion

#region  Character Selection
        private void userInteraction_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _userInteraction = true;
        }

        private void cbCharType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             if (Tree == null)
                return;
             if (!_userInteraction)
                 return;
             if (Tree.Chartype == cbCharType.SelectedIndex) return;

            Tree.SwitchClass(cbCharType.SelectedIndex);
            UpdateUI();
            SetCurrentBuildUrlFromTree();
            _userInteraction = false;
            cbAscType.SelectedIndex = 0;
        }

        private void cbAscType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_userInteraction)
                return;
            if (cbAscType.SelectedIndex < 0 || cbAscType.SelectedIndex > 3)
                return;

            Tree.AscType = cbAscType.SelectedIndex;

            UpdateUI();
            SetCurrentBuildUrlFromTree();
            _userInteraction = false;
        }

        private void PopulateAsendancySelectionList()
        {
            if (!Tree.UpdateAscendancyClasses) return;
            Tree.UpdateAscendancyClasses = false;
            var ascendancyItems = new List<string> { "None" };
            foreach (var name in Tree.AscClasses.GetClasses(Tree.Chartype))
                ascendancyItems.Add(name.DisplayName);
            cbAscType.ItemsSource = ascendancyItems.Select(x => new ComboBoxItem { Name = x, Content = x });
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
            SetCurrentBuildUrlFromTree();
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
            UpdatePoints();
        }

        public void UpdateAllAttributeList()
        {
            lbAllAttr.SelectedIndex = -1;
            _allAttributesList.Clear();

            if (_itemAttributes == null) return;

            var attritemp = Tree.SelectedAttributesWithoutImplicit;

            var itemAttris = _itemAttributes.NonLocalMods
                .Select(m => new KeyValuePair<string, List<float>>(m.Attribute, m.Value))
                .SelectMany(SkillTree.ExpandHybridAttributes);
            foreach (var mod in itemAttris)
            {
                if (attritemp.ContainsKey(mod.Key))
                {
                    for (var i = 0; i < mod.Value.Count; i++)
                    {
                        attritemp[mod.Key][i] += mod.Value[i];
                    }
                }
                else
                {
                    attritemp[mod.Key] = new List<float>(mod.Value);
                }
            }

            foreach (var a in SkillTree.ImplicitAttributes(attritemp, Tree.Level))
            {
                var key = SkillTree.RenameImplicitAttributes.ContainsKey(a.Key)
                    ? SkillTree.RenameImplicitAttributes[a.Key]
                    : a.Key;

                if (!attritemp.ContainsKey(key))
                    attritemp[key] = new List<float>();
                for (var i = 0; i < a.Value.Count; i++)
                {
                    if (attritemp.ContainsKey(key) && attritemp[key].Count > i)
                        attritemp[key][i] += a.Value[i];
                    else
                    {
                        attritemp[key].Add(a.Value[i]);
                    }
                }
            }
            
            foreach (var item in (attritemp.Select(InsertNumbersInAttributes)))
            {
                var a = new Attribute(item);
                if (!CheckIfAttributeMatchesFilter(a)) continue;
                _allAttributesList.Add(a);
            }
        }

        public void UpdateClass()
        {
            cbCharType.SelectedIndex = Tree.Chartype;
            cbAscType.SelectedIndex = Tree.AscType;
        }

        public void UpdateAttributeList()
        {
            lbAttr.SelectedIndex = -1;
            _attiblist.Clear();
            var copy = Tree.HighlightedAttributes == null
                ? null
                : new Dictionary<string, List<float>>(Tree.HighlightedAttributes);
            
            foreach (var item in Tree.SelectedAttributes)
            {
                var a = new Attribute(InsertNumbersInAttributes(item));
                if (!CheckIfAttributeMatchesFilter(a)) continue;
                if (copy != null && copy.ContainsKey(item.Key))
                {
                    var citem = copy[item.Key];
                    a.Deltas = item.Value.Zip(citem, (s, h) => s - h).ToArray();
                    copy.Remove(item.Key);
                }
                else
                {
                    a.Deltas = copy != null ? item.Value.ToArray() : item.Value.Select(v => 0f).ToArray();
                }
                _attiblist.Add(a);
            }

            if (copy != null)
            {
                foreach (var item in copy)
                {
                    var a = new Attribute(InsertNumbersInAttributes(new KeyValuePair<string, List<float>>(item.Key, item.Value.Select(v => 0f).ToList())));
                    if (!CheckIfAttributeMatchesFilter(a)) continue;
                    a.Deltas = item.Value.Select(h => 0 - h).ToArray();
                    a.Missing = true;
                    _attiblist.Add(a);
                }
            }
        }

        public void UpdatePoints()
        {
            var points = Tree.GetPointCount();
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

                foreach (var group in Compute.Defense())
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

                foreach (var group in Compute.Offense())
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
            var s = attrib.Key;
            foreach (var f in attrib.Value)
            {
                s = _backreplace.Replace(s, f + "", 1);
            }
            return s;
        }

        private bool CheckIfAttributeMatchesFilter(Attribute a)
        {
            var filter = tbAttributesFilter.Text;
            if (cbAttributesFilterRegEx.IsChecked == true)
            {
                try
                {
                    var regex = new Regex(filter, RegexOptions.IgnoreCase);
                    if (!regex.IsMatch(a.Text)) return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else if (!a.Text.Contains(filter, StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }

#endregion

#region Attribute and Character lists - Event Handlers

        private void ToggleAttributes()
        {
            PersistentData.Options.AttributesBarOpened = !PersistentData.Options.AttributesBarOpened;
        }

        private void ToggleAttributes(bool expanded)
        {
            PersistentData.Options.AttributesBarOpened = expanded;
        }

        private void ToggleCharacterSheet()
        {
            PersistentData.Options.CharacterSheetBarOpened = !PersistentData.Options.CharacterSheetBarOpened;
        }

        private void ToggleCharacterSheet(bool expanded)
        {
            PersistentData.Options.CharacterSheetBarOpened = expanded;
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
            PersistentData.Options.BuildsBarOpened = !PersistentData.Options.BuildsBarOpened;
        }

        private void tbAttributesFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAttributeLists();
        }

        private void cbAttributesFilterRegEx_Click(object sender, RoutedEventArgs e)
        {
            FilterAttributeLists();
        }

        private void FilterAttributeLists()
        {
            if (cbAttributesFilterRegEx.IsChecked == true && !RegexTools.IsValidRegex(tbAttributesFilter.Text)) return;
            UpdateAllAttributeList();
            UpdateAttributeList();
            RefreshAttributeLists();
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabItem1.IsSelected || tabItem3.IsSelected)
                gAttributesFilter.Visibility = Visibility.Visible;
            else
                gAttributesFilter.Visibility = Visibility.Collapsed;
        }

#endregion

#region zbSkillTreeBackground

        private void zbSkillTreeBackground_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _lastMouseButton = e.ChangedButton;
        }

        private void zbSkillTreeBackground_Click(object sender, RoutedEventArgs e)
        {
            var p = ((MouseEventArgs)e.OriginalSource).GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;

            var node = Tree.FindNodeInRange(v, 50);
            if (node != null && !SkillTree.RootNodeList.Contains(node.Id))
            {
                if (node.ascendancyName != null && !Tree.DrawAscendancy)
                    return;
                var ascendancyClassName = Tree.AscClasses.GetClassName(Tree.Chartype, Tree.AscType);
                if (!PersistentData.Options.ShowAllAscendancyClasses && node.ascendancyName != null && node.ascendancyName != ascendancyClassName)
                    return;
                // Ignore clicks on character portraits and masteries
                if (node.Spc == null && node.Type != NodeType.Mastery)
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
                        if (Tree.SkilledNodes.Contains(node))
                        {
                            Tree.ForceRefundNode(node);
                            _prePath = Tree.GetShortestPathTo(node, Tree.SkilledNodes);
                            Tree.DrawPath(_prePath);
                        }
                        else if (_prePath != null)
                        {
                            Tree.AllocateSkillNodes(_prePath);
                            _toRemove = Tree.ForceRefundNodePreview(node);
                            if (_toRemove != null)
                                Tree.DrawRefundPreview(_toRemove);
                        }
                    }
                }
                SetCurrentBuildUrlFromTree();
                UpdateUI();
            }
            else if ((Tree.AscButtonPosition - v).Length < 150 && Tree.AscType != 0)
            {
                if (PersistentData.Options.ShowAllAscendancyClasses) return;
                Tree.DrawAscendancyButton("Pressed");
                Tree.ToggleAscendancyTree();
                SearchUpdate();
            }
            else
            {
                var size = zbSkillTreeBackground.Child.DesiredSize;
                if (p.X < 0 || p.Y < 0 || p.X > size.Width || p.Y > size.Height)
                {
                    if (_lastMouseButton == MouseButton.Right)
                        zbSkillTreeBackground.Reset();
                }
            }
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
            var p = e.GetPosition(zbSkillTreeBackground.Child);
            var v = new Vector2D(p.X, p.Y);
            v = v * _multransform + _addtransform;

            var node = Tree.FindNodeInRange(v, 50);
            _hoveredNode = node;
            if (node != null && !SkillTree.RootNodeList.Contains(node.Id))
            {         
                if (!Tree.DrawAscendancy && node.ascendancyName != null)
                    return;
                if (!PersistentData.Options.ShowAllAscendancyClasses && node.ascendancyName != null && node.ascendancyName != Tree.AscClasses.GetClassName(Tree.Chartype, Tree.AscType))
                    return;
                if (node.Type == NodeType.JewelSocket)
                {
                    Tree.DrawJewelHighlight(node);
                }
                
                if (Tree.SkilledNodes.Contains(node))
                {
                    _toRemove = Tree.ForceRefundNodePreview(node);
                    if (_toRemove != null)
                        Tree.DrawRefundPreview(_toRemove);
                }
                else
                {
                    _prePath = Tree.GetShortestPathTo(node, Tree.SkilledNodes);
                    if (node.Type != NodeType.Mastery)
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
                    if (_prePath != null && node.Type != NodeType.Mastery)
                    {
                        var points = _prePath.Count;
                        if(_prePath.Any(x => x.IsAscendancyStart))
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
            else if ((Tree.AscButtonPosition - v).Length < 150)
            {
                Tree.DrawAscendancyButton("Highlight");
            }
            else
            {
                _sToolTip.Tag = false;
                _sToolTip.IsOpen = false;
                _prePath = null;
                _toRemove = null;
                Tree?.ClearPath();
                Tree?.ClearJewelHighlight();
                Tree?.DrawAscendancyButton();
            }
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

        private bool _pauseLoadItemData;

        private async Task LoadItemData()
        {
            if (_pauseLoadItemData)
                return;

            if (ItemAttributes != null)
            {
                ItemAttributes.Equip.CollectionChanged -= ItemAttributesEquipCollectionChanged;
                ItemAttributes.PropertyChanged -= ItemAttributesPropertyChanged;
            }

            var itemData = PersistentData.CurrentBuild.ItemData;
            ItemAttributes itemAttributes;
            if (!string.IsNullOrEmpty(itemData))
            {
                try
                {
                    itemAttributes = new ItemAttributes(PersistentData, itemData);
                }
                catch (Exception ex)
                {
                    itemAttributes = new ItemAttributes();
                    await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load item data."),
                        ex.Message);
                }
            }
            else
            {
                itemAttributes = new ItemAttributes();
            }

            itemAttributes.Equip.CollectionChanged += ItemAttributesEquipCollectionChanged;
            itemAttributes.PropertyChanged += ItemAttributesPropertyChanged;
            ItemAttributes = itemAttributes;
            UpdateUI();
        }

        private void ItemAttributesPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            UpdateUI();
        }

        private void ItemAttributesEquipCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            _pauseLoadItemData = true;
            PersistentData.CurrentBuild.ItemData = ItemAttributes.ToJsonString();
            _pauseLoadItemData = false;
        }

#endregion

#region Builds - Services

        private async Task CurrentBuildChanged()
        {
            var build = PersistentData.CurrentBuild;
            Tree.Level = build.Level;
            Tree.ResetTaggedNodes();
            TreeGeneratorInteraction?.LoadSettings();
            await LoadItemData();
            SetCustomGroups(build.CustomGroups);
            await LoadBuildFromUrlAsync();
        }

        /// <summary>
        /// Call this to set CurrentBuild.TreeUrl when there were direct SkillTree changes.
        /// </summary>
        private void SetCurrentBuildUrlFromTree()
        {
            _skipLoadOnCurrentBuildTreeChange = true;
            PersistentData.CurrentBuild.TreeUrl = Tree.SaveToUrl();
            _skipLoadOnCurrentBuildTreeChange = false;
        }

        private Task LoadBuildFromUrlAsync()
        {
            return LoadBuildFromUrlAsync(PersistentData.CurrentBuild.TreeUrl);
        }

        private async Task LoadBuildFromUrlAsync(string treeUrl)
        {
            try
            {
                if (treeUrl.Contains("poezone.ru"))
                {
                    await SkillTreeImporter.LoadBuildFromPoezone(DialogCoordinator.Instance, Tree, treeUrl);
                    SetCurrentBuildUrlFromTree();
                }
                else if (treeUrl.Contains("google.com"))
                {
                    var match = Regex.Match(treeUrl, @"q=(.*?)&");
                    if (match.Success)
                    {
                        var newUrl = match.ToString().Replace("q=", "").Replace("&", "");
                        await LoadBuildFromUrlAsync(newUrl);
                        return;
                    }
                    else
                        throw new Exception("The URL you are trying to load is invalid.");
                }
                else if (treeUrl.Contains("tinyurl.com") || treeUrl.Contains("poeurl.com"))
                {
                    var skillUrl = treeUrl.Replace("preview.", "");
                    if (skillUrl.Contains("poeurl.com") && !skillUrl.Contains("redirect.php"))
                    {
                        skillUrl = skillUrl.Replace("http://poeurl.com/",
                            "http://poeurl.com/redirect.php?url=");
                    }

                    var response =
                        await AwaitAsyncTask(L10n.Message("Resolving shortened tree address"),
                            new HttpClient().GetAsync(skillUrl, HttpCompletionOption.ResponseHeadersRead));
                    response.EnsureSuccessStatusCode();
                    if (Regex.IsMatch(response.RequestMessage.RequestUri.ToString(), Constants.TreeRegex))
                    {
                        var newUrl = response.RequestMessage.RequestUri.ToString();
                        await LoadBuildFromUrlAsync(newUrl);
                        return;
                    }
                    else
                        throw new Exception("The URL you are trying to load is invalid.");
                }
                else
                {
                    var newUrl = treeUrl;
                    if (newUrl.Contains("characterName") || newUrl.Contains("accountName"))
                        newUrl = Regex.Replace(newUrl, @"\?.*", "");
                    newUrl = Regex.Replace(newUrl, Constants.TreeRegex, Constants.TreeAddress);
                    // If the url did change, it'll run through this method again anyway.
                    // So no need to call Tree.LoadFromUrl in that case.
                    if (PersistentData.CurrentBuild.TreeUrl == newUrl)
                        Tree.LoadFromUrl(newUrl);
                    else
                        PersistentData.CurrentBuild.TreeUrl = newUrl;
                }

                if (_justLoaded)
                {
                    if (_undoList.Count > 1)
                    {
                        var holder = _undoList.Pop();
                        while (_undoList.Count != 0)
                            _undoList.Pop();
                        _undoList.Push(holder);
                    }
                }
                else
                {
                    UpdateClass();
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAsendancySelectionList();
                }
                UpdateUI();
                _justLoaded = false;
            }
            catch (Exception ex)
            {
                PersistentData.CurrentBuild.TreeUrl = Tree.SaveToUrl();
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
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

        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            _undoList.Push(PersistentData.CurrentBuild.TreeUrl);
        }

        private void tbSkillURL_Undo_Click(object sender, RoutedEventArgs e)
        {
            tbSkillURL_Undo();
        }

        private void tbSkillURL_Undo()
        {
            if (_undoList.Count <= 0) return;
            if (_undoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _undoList.Count > 1)
            {
                _undoList.Pop();
                tbSkillURL_Undo();
            }
            else if (_undoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                _redoList.Push(PersistentData.CurrentBuild.TreeUrl);
                PersistentData.CurrentBuild.TreeUrl = _undoList.Pop();
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
            if (_redoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _redoList.Count > 1)
            {
                _redoList.Pop();
                tbSkillURL_Redo();
            }
            else if (_redoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                PersistentData.CurrentBuild.TreeUrl = _redoList.Pop();
                UpdateUI();
            }
        }

        private async void btnPoeUrl_Click(object sender, RoutedEventArgs e)
        {
            await DownloadPoeUrlAsync();
        }

        private async Task DownloadPoeUrlAsync()
        {
            var regx =
                new Regex(
                    "https?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?",
                    RegexOptions.IgnoreCase);

            var matches = regx.Matches(PersistentData.CurrentBuild.TreeUrl);

            if (matches.Count == 1)
            {
                try
                {
                    var url = matches[0].ToString();
                    if (!url.ToLower().StartsWith(Constants.TreeAddress))
                    {
                        return;
                    }
                    // PoEUrl can't handle https atm.
                    url = url.Replace("https://", "http://");

                    var result =
                        await AwaitAsyncTask(L10n.Message("Generating PoEUrl of Skill tree"),
                            new HttpClient().GetStringAsync("http://poeurl.com/shrink.php?url=" + url));
                    await ShowPoeUrlMessageAndAddToClipboard("http://poeurl.com/" + result.Trim());
                }
                catch (Exception ex)
                {
                    await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to contact the PoEUrl location."), ex.Message);
                }
            }
        }

        private async Task ShowPoeUrlMessageAndAddToClipboard(string poeurl)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(poeurl, true);
                await this.ShowInfoAsync(L10n.Message("The PoEUrl link has been copied to Clipboard.") + "\n\n" + poeurl);
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync(L10n.Message("An error occurred while copying to Clipboard."), ex.Message);
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
            var accent = ThemeManager.Accents.First(x => Equals(x.Name, PersistentData.Options.Accent));
            var theme = ThemeManager.GetAppTheme("Base" + sTheme);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            ((MenuItem)NameScope.GetNameScope(this).FindName("mnuViewTheme" + sTheme)).IsChecked = true;
            PersistentData.Options.Theme = sTheme;
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
            var theme = ThemeManager.GetAppTheme("Base" + PersistentData.Options.Theme);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
            ((MenuItem)NameScope.GetNameScope(this).FindName("mnuViewAccent" + sAccent)).IsChecked = true;
            PersistentData.Options.Accent = sAccent;
        }
#endregion

        private void UpdateTreeComparision()
        {
            if (Tree == null)
                return;

            var build = PersistentData.SelectedBuild as PoEBuild;
            if (build != null && PersistentData.Options.TreeComparisonEnabled)
            {
                HashSet<SkillNode> nodes;
                int ctype;
                int atype;
                SkillTree.DecodeUrl(build.TreeUrl, out nodes, out ctype, out atype);

                Tree.HighlightedNodes.Clear();
                Tree.HighlightedNodes.UnionWith(nodes);
                Tree.HighlightedAttributes = SkillTree.GetAttributes(nodes, ctype, build.Level, build.Bandits);
            }
            else
            {
                Tree.HighlightedNodes.Clear();
                Tree.HighlightedAttributes = null;
            }
            UpdateUI();
        }

        private async void Button_Craft_Click(object sender, RoutedEventArgs e)
        {
            var w = new CraftWindow(PersistentData.EquipmentData);
            await this.ShowDialogAsync(new CraftViewModel(), w);
            if (!w.DialogResult) return;

            var item = w.Item;
            if (PersistentData.StashItems.Count > 0)
                item.Y = PersistentData.StashItems.Max(i => i.Y + i.Height);

            Stash.Items.Add(item);

            Stash.AddHighlightRange(new IntRange() { From = item.Y, Range = item.Height });
            Stash.asBar.Value = item.Y;
        }

        private static DragDropEffects deleteRect_DropEffect(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DraggedItem)))
            {
                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                var effect = draggedItem.DropOnBinEffect;

                if (e.AllowedEffects.HasFlag(effect))
                {
                    return effect;
                }
            }
            return DragDropEffects.None;
        }

        private void deleteRect_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = deleteRect_DropEffect(e);
        }

        private void deleteRect_Drop(object sender, DragEventArgs e)
        {
            var effect = deleteRect_DropEffect(e);
            if (effect == DragDropEffects.None)
                return;

            e.Handled = true;
            e.Effects = effect;
            var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
            var visualizer = draggedItem.SourceItemVisualizer;
            var st = visualizer.TryFindParent<Stash>();
            if (st != null)
            {
                st.RemoveItem(visualizer.Item);
            }
            else
            {
                visualizer.Item = null;
            }
            deleteRect.Opacity = 0.0;
        }

        private void deleteRect_DragEnter(object sender, DragEventArgs e)
        {
            if (deleteRect_DropEffect(e) != DragDropEffects.None)
            {
                deleteRect.Opacity = 0.3;
            }
        }

        private void deleteRect_DragLeave(object sender, DragEventArgs e)
        {
            if (deleteRect_DropEffect(e) != DragDropEffects.None)
            {
                deleteRect.Opacity = 0.0;
            }
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
            if (WindowPlacementSettings != null) return settings;

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
            return settings;
        }
    }
}