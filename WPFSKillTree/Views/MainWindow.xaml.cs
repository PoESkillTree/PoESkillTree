using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnumsNET;
using Fluent;
using MahApps.Metro.Controls;
using NLog;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Utils;
using PoESkillTree.Computation;
using PoESkillTree.Computation.ViewModels;
using PoESkillTree.Computation.Views;
using PoESkillTree.Controls;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.ViewModels;
using PoESkillTree.Utils.Converter;
using PoESkillTree.Utils.UrlProcessing;
using PoESkillTree.ViewModels;
using PoESkillTree.ViewModels.Builds;
using PoESkillTree.ViewModels.Crafting;
using PoESkillTree.ViewModels.Equipment;
using PoESkillTree.ViewModels.Import;
using PoESkillTree.ViewModels.Skills;
using PoESkillTree.Views.Crafting;
using PoESkillTree.Views.Import;
using Attribute = PoESkillTree.ViewModels.Attribute;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Item = PoESkillTree.Model.Items.Item;
using MenuItem = System.Windows.Controls.MenuItem;
using OnThemeChangedEventArgs = MahApps.Metro.OnThemeChangedEventArgs;
using ThemeManager = MahApps.Metro.ThemeManager;

namespace PoESkillTree.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged, IRibbonWindow
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The set of keys of which one needs to be pressed to highlight similar nodes on hover.
        /// </summary>
        private static readonly Key[] HighlightByHoverKeys = { Key.LeftShift, Key.RightShift };

        public event PropertyChangedEventHandler? PropertyChanged;

        private IExtendedDialogCoordinator _dialogCoordinator;
        public IPersistentData PersistentData { get; } = App.PersistentData;

        private readonly List<Attribute> _attiblist = new List<Attribute>();
        private readonly Regex _backreplace = new Regex("#");
        private readonly ToolTip _sToolTip = new ToolTip();
        private readonly BuildUrlNormalizer _buildUrlNormalizer = new BuildUrlNormalizer();
        private ListCollectionView _attributeCollection;

        private GroupStringConverter _attributeGroups;
        private ContextMenu _attributeContextMenu;
        private MenuItem cmAddToGroup, cmDeleteGroup;

        private readonly HttpClient _httpClient = new HttpClient();

        private GameData _gameData;

        private ItemAttributes _itemAttributes;

        private ItemAttributes ItemAttributes
        {
            get => _itemAttributes;
            set => SetProperty(ref _itemAttributes, value);
        }

        private InventoryViewModel _inventoryViewModel;
        public InventoryViewModel InventoryViewModel
        {
            get => _inventoryViewModel;
            private set => SetProperty(ref _inventoryViewModel, value);
        }

        private SkillTreeAreaViewModel _skillTreeAreaViewModel;
        public SkillTreeAreaViewModel SkillTreeAreaViewModel
        {
            get => _skillTreeAreaViewModel;
            private set => SetProperty(ref _skillTreeAreaViewModel, value);
        }

        private JewelSocketObserver _jewelSocketObserver;
        private AbyssalSocketObserver _abyssalSocketObserver;

        public StashViewModel StashViewModel { get; } = new StashViewModel();

        private ImportViewModels _importViewModels;

        private ObservableItemCollectionConverter? _equipmentConverter;

        private ComputationViewModel? _computationViewModel;

        [DisallowNull]
        public ComputationViewModel? ComputationViewModel
        {
            get => _computationViewModel;
            private set
            {
                value!.SharedConfiguration.SetLevel(PersistentData.CurrentBuild.Level);
                value.SharedConfiguration.SetCharacterClass(Tree.CharClass);
                value.SharedConfiguration.SetBandit(PersistentData.CurrentBuild.Bandits.Choice);
                SetProperty(ref _computationViewModel, value);
            }
        }

        private SkillsEditingViewModel _skillsEditingViewModel;

        public SkillsEditingViewModel SkillsEditingViewModel
        {
            get => _skillsEditingViewModel;
            private set => SetProperty(ref _skillsEditingViewModel, value);
        }

        private SkillTree _tree;
        public SkillTree Tree
        {
            get => _tree;
            private set
            {
                if (_tree != null)
                    _tree.PropertyChanged -= Tree_PropertyChanged;
                SetProperty(ref _tree!, value);
            }
        }
        private async Task<SkillTree> CreateSkillTreeAsync(ProgressDialogController controller,
            AssetLoader? assetLoader = null)
        {
            var tree = await SkillTree.CreateAsync(PersistentData, controller, assetLoader);
            tree.PropertyChanged += Tree_PropertyChanged;
            if (BuildsControlViewModel != null)
                BuildsControlViewModel.SkillTree = tree;
            if (TreeGeneratorInteraction != null)
                TreeGeneratorInteraction.SkillTree = tree;
            if (InventoryViewModel != null)
            {
                tree.JewelViewModels = InventoryViewModel.TreeJewels;
                SkillTreeAreaViewModel.Dispose();
                SkillTreeAreaViewModel = new SkillTreeAreaViewModel(SkillTree.Skillnodes, InventoryViewModel.TreeJewels);
            }
            if (ComputationViewModel != null)
            {
                tree.ItemConnectedNodesSelector = ComputationViewModel.PassiveTreeConnections.GetConnectedNodes;
            }
            _jewelSocketObserver?.Dispose();
            _jewelSocketObserver = new JewelSocketObserver(tree.SkilledNodes);
            return tree;
        }

        private BuildsControlViewModel _buildsControlViewModel;

        public BuildsControlViewModel BuildsControlViewModel
        {
            get => _buildsControlViewModel;
            private set => SetProperty(ref _buildsControlViewModel, value);
        }

        public CommandCollectionViewModel LoadTreeButtonViewModel { get; } = new CommandCollectionViewModel();

        private Vector2D _addtransform;
        private bool _justLoaded;
        private string? _lasttooltip;

        private Vector2D _multransform;

        private IReadOnlyCollection<SkillNode>? _prePath;
        private IReadOnlyCollection<SkillNode>? _toRemove;

        private readonly Stack<string> _undoList = new Stack<string>();
        private readonly Stack<string> _redoList = new Stack<string>();

        private MouseButton _lastMouseButton;
        private bool _userInteraction;
        /// <summary>
        /// The node of the SkillTree that currently has the mouse over it.
        /// Null if no node is under the mouse.
        /// </summary>
        private SkillNode? _hoveredNode;

        private SkillNode? _lastHoveredNode;

        private bool _noAsyncTaskRunning = true;
        /// <summary>
        /// Specifies if there is a task running asynchronously in the background.
        /// Used to disable UI buttons that might interfere with the result of the task.
        /// </summary>
        public bool NoAsyncTaskRunning
        {
            get => _noAsyncTaskRunning;
            private set => SetProperty(ref _noAsyncTaskRunning, value);
        }

        private TreeGeneratorInteraction? _treeGeneratorInteraction;

        [DisallowNull]
        public TreeGeneratorInteraction? TreeGeneratorInteraction
        {
            get => _treeGeneratorInteraction;
            private set => SetProperty(ref _treeGeneratorInteraction, value);
        }

        /// <summary>
        /// Set to true when CurrentBuild.TreeUrl was set after direct SkillTree changes so the SkillTree
        /// doesn't need to be reloaded.
        /// </summary>
        private bool _skipLoadOnCurrentBuildTreeChange;

        private string? _inputTreeUrl;
        /// <summary>
        /// The tree url that is the current input of the tree text box. Can be different from
        /// CurrentBuild.TreeUrl if the user changes it (until the user presses "Load Tree" or Enter).
        /// </summary>
        [DisallowNull]
        public string? InputTreeUrl
        {
            get => _inputTreeUrl;
            set => SetProperty(ref _inputTreeUrl, value);
        }

        public ICommand UndoTreeUrlChangeCommand { get; }
        public ICommand RedoTreeUrlChangeCommand { get; }

        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(
            "TitleBar", typeof(RibbonTitleBar), typeof(MainWindow), new PropertyMetadata(default(RibbonTitleBar)));

        public RibbonTitleBar TitleBar
        {
            get => (RibbonTitleBar) GetValue(TitleBarProperty);
            private set => SetValue(TitleBarProperty, value);
        }

#pragma warning disable CS8618 // Initialized in Window_Loaded
        public MainWindow()
#pragma warning restore
        {
            InitializeComponent();

            UndoTreeUrlChangeCommand = new RelayCommand(UndoTreeUrlChange, CanUndoTreeUrlChange);
            RedoTreeUrlChangeCommand = new RelayCommand(RedoTreeUrlChange, CanRedoTreeUrlChange);
        }

        private void SetProperty<T>(
            ref T backingStore, T value, Action? onChanged = null, [CallerMemberName] string propertyName = "Unspecified")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            backingStore = value;

            onChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RegisterPersistentDataHandlers()
        {
            // Register handlers
            PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
            PersistentData.CurrentBuild.Bandits.PropertyChanged += CurrentBuildOnPropertyChanged;
            // Re-register handlers when PersistentData.CurrentBuild is set.
            PersistentData.PropertyChanged += async (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PersistentData.CurrentBuild):
                        PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
                        PersistentData.CurrentBuild.Bandits.PropertyChanged += CurrentBuildOnPropertyChanged;
                        await CurrentBuildChanged();
                        break;
                    case nameof(PersistentData.SelectedBuild):
                        UpdateTreeComparison();
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
                    PersistentData.CurrentBuild.Bandits.PropertyChanged -= CurrentBuildOnPropertyChanged;
                }
            };
        }

        private async void CurrentBuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(PoEBuild.ItemData):
                    await LoadItemData();
                    _jewelSocketObserver.SetTreeJewelViewModels(InventoryViewModel.TreeJewels);
                    break;
                case nameof(PoEBuild.TreeUrl):
                    if (!_skipLoadOnCurrentBuildTreeChange)
                        await SetTreeUrl(PersistentData.CurrentBuild.TreeUrl);
                    InputTreeUrl = PersistentData.CurrentBuild.TreeUrl;
                    break;
                case nameof(PoEBuild.CheckedNodeIds):
                case nameof(PoEBuild.CrossedNodeIds):
                    Tree.ResetTaggedNodes();
                    break;
                case nameof(PoEBuild.AdditionalData):
                    TreeGeneratorInteraction?.LoadSettings();
                    break;
                case nameof(BanditSettings.Choice):
                    UpdateUI();
                    ComputationViewModel?.SharedConfiguration.SetBandit(PersistentData.CurrentBuild.Bandits.Choice);
                    break;
                case nameof(PoEBuild.Level):
                    ComputationViewModel?.SharedConfiguration.SetLevel(PersistentData.CurrentBuild.Level);
                    break;
            }
        }

        //This whole region, along with most of GroupStringConverter, makes up our user-defined attribute group functionality - Sectoidfodder 02/29/16
        #region Attribute grouping helpers

        //Necessary to update the summed numbers in group names before every refresh
        private void RefreshAttributeLists()
        {
            _attributeGroups.UpdateGroupNames(_attiblist);
            _attributeCollection.Refresh();
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
            var attributelist = new List<string>();
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())
            {
                attributelist.Add(o.ToString());
            }

            //Build and show form to enter group name
            var name = await ExtendedDialogManager.ShowInputAsync(this, L10n.Message("Create New Attribute Group"), L10n.Message("Group name"));
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
            var attributelist = new List<string>();
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())
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
            var attributelist = new List<string>();
            foreach (var o in lbAttr.SelectedItems.Cast<Attribute>())
            {
                attributelist.Add(o.ToString());
            }
            if (attributelist.Count > 0)
            {
                _attributeGroups.AddGroup(((MenuItem)sender).Header.ToString()!, attributelist.ToArray());
                RefreshAttributeLists();
            }
        }

        //Deletes the entire custom group named by sender.Header, restoring all contained attributes to their default groups
        private void DeleteGroup(object sender, RoutedEventArgs e)
        {
            //Remove submenus that work with the group
            for (var i = 0; i < cmAddToGroup.Items.Count; i++)
            {
                if (((MenuItem)cmAddToGroup.Items[i]).Header.ToString()!.ToLower().Equals(((MenuItem)sender).Header.ToString()!.ToLower()))
                {
                    cmAddToGroup.Items.RemoveAt(i);
                    if (cmAddToGroup.Items.Count == 0)
                        cmAddToGroup.IsEnabled = false;
                    break;
                }
            }
            for (var i = 0; i < cmDeleteGroup.Items.Count; i++)
            {
                if (((MenuItem)cmDeleteGroup.Items[i]).Header.ToString()!.ToLower().Equals(((MenuItem)sender).Header.ToString()!.ToLower()))
                {
                    cmDeleteGroup.Items.RemoveAt(i);
                    if (cmDeleteGroup.Items.Count == 0)
                        cmDeleteGroup.IsEnabled = false;
                    break;
                }
            }

            _attributeGroups.DeleteGroup(((MenuItem)sender).Header.ToString()!);
            RefreshAttributeLists();
        }

        #endregion

        #region Window methods

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            var controller = await this.ShowProgressAsync(L10n.Message("Initialization"),
                        L10n.Message("Initializing window ..."));
            controller.Maximum = 1;
            controller.SetIndeterminate();

            var computationInitializer = ComputationInitializer.StartNew();
            _gameData = computationInitializer.GameData;
            var persistentDataTask = PersistentData.InitializeAsync(DialogCoordinator.Instance);

            InitializeIndependentUI();
            Log.Info($"Independent UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            await persistentDataTask;
            InitializePersistentDataDependentUI();
            Log.Info($"PersistentData UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            controller.SetMessage(L10n.Message("Loading skill tree assets ..."));
            Tree = await CreateSkillTreeAsync(controller);
            InitializeTreeDependentUI();
            Log.Info($"Tree UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            controller.SetMessage(L10n.Message("Initializing window ..."));
            controller.SetIndeterminate();
            await Task.Delay(1); // Give the progress dialog a chance to update

            var initialComputationTask = computationInitializer.InitializeAsync(SkillTree.Skillnodes.Values);

            _justLoaded = true;
            // loading last build
            await CurrentBuildChanged();
            _justLoaded = false;
            InitializeBuildDependentUI();
            Log.Info($"Build UI initialized after {stopwatch.ElapsedMilliseconds} ms");
            
            await initialComputationTask;
            await InitializeComputationDependentAsync(computationInitializer);
            Log.Info($"Computation UI initialized after {stopwatch.ElapsedMilliseconds} ms");

            await controller.CloseAsync();

            stopwatch.Stop();
            Log.Info($"Window_Loaded took {stopwatch.ElapsedMilliseconds} ms");
        }

        private void InitializeIndependentUI()
        {
            TitleBar = this.FindChild<RibbonTitleBar>("RibbonTitleBar");
            TitleBar.InvalidateArrange();
            TitleBar.UpdateLayout();

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
            var cmCreateGroup = new MenuItem { Header = "Create new group" };
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
            var cmRemoveFromGroup = new MenuItem { Header = "Remove from group" };
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
            _attributeCollection.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Attribute.Text), _attributeGroups));
            _attributeCollection.CustomSort = _attributeGroups;
            lbAttr.ItemsSource = _attributeCollection;
            lbAttr.SelectionMode = SelectionMode.Extended;
            lbAttr.ContextMenu = _attributeContextMenu;

            cbCharType.ItemsSource = Enums.GetValues<CharacterClass>();
            cbAscType.SelectedIndex = 0;
        }

        private void InitializePersistentDataDependentUI()
        {
            _dialogCoordinator = new ExtendedDialogCoordinator(_gameData, PersistentData);
            RegisterPersistentDataHandlers();
            StashViewModel.Initialize(_dialogCoordinator, PersistentData);
            _importViewModels = new ImportViewModels(_dialogCoordinator, PersistentData, StashViewModel);
            InitializeTheme();
        }

        private void InitializeTreeDependentUI()
        {
            updateCanvasSize();
            recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);
        }

        private void InitializeBuildDependentUI()
        {
            PersistentData.Options.PropertyChanged += Options_PropertyChanged;
            PopulateAscendancySelectionList();
            BuildsControlViewModel = new BuildsControlViewModel(_dialogCoordinator, PersistentData, Tree, _httpClient);
            UpdateTreeComparison();
            TreeGeneratorInteraction =
                new TreeGeneratorInteraction(SettingsDialogCoordinator.Instance, PersistentData, Tree);
            TreeGeneratorInteraction.RunFinished += (o, args) =>
            {
                UpdateUI();
                SetCurrentBuildUrlFromTree();
            };

            InitializeLoadTreeButtonViewModel();
        }

        private void InitializeLoadTreeButtonViewModel()
        {
            LoadTreeButtonViewModel.Add(L10n.Message("Load Tree"), async () =>
            {
                if (string.IsNullOrWhiteSpace(InputTreeUrl))
                    return;
                await LoadBuildFromUrlAsync(InputTreeUrl);
            }, () => NoAsyncTaskRunning);
            LoadTreeButtonViewModel.Add(L10n.Message("Load as new build"), async () =>
            {
                if (string.IsNullOrWhiteSpace(InputTreeUrl))
                    return;

                var url = InputTreeUrl;
                BuildsControlViewModel.NewBuild(BuildsControlViewModel.BuildRoot);
                await LoadBuildFromUrlAsync(url, forceBanditsUpdate: true);
            }, () => NoAsyncTaskRunning);
            LoadTreeButtonViewModel.SelectedIndex = PersistentData.Options.LoadTreeButtonIndex;
            LoadTreeButtonViewModel.PropertyChanged += (o, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(LoadTreeButtonViewModel.SelectedItem):
                        PersistentData.Options.LoadTreeButtonIndex = LoadTreeButtonViewModel.SelectedIndex;
                        break;
                }
            };
        }

        private async Task InitializeComputationDependentAsync(ComputationInitializer computationInitializer)
        {
            _equipmentConverter = new ObservableItemCollectionConverter(computationInitializer.CreateAdditionalSkillStatApplier());
            _equipmentConverter.ConvertFrom(ItemAttributes);
            await computationInitializer.InitializeAfterBuildLoadAsync(
                Tree.SkilledNodes,
                _equipmentConverter.Equipment,
                _equipmentConverter.Jewels,
                _equipmentConverter.Gems,
                _equipmentConverter.Skills);
            computationInitializer.SetupPeriodicActions();
            ComputationViewModel = await computationInitializer.CreateComputationViewModelAsync(PersistentData);
            Tree.ItemConnectedNodesSelector = ComputationViewModel.PassiveTreeConnections.GetConnectedNodes;
            _abyssalSocketObserver = computationInitializer.CreateAbyssalSocketObserver(InventoryViewModel.ItemJewels);
            (await computationInitializer.CreateItemAllocatedPassiveNodesObservableAsync()).Subscribe(
                nodes => Tree.ItemAllocatedNodes = nodes,
                ex => Log.Error(ex, "Error in ItemAllocatedPassiveNodesObservable"));
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Options.ShowAllAscendancyClasses):
                    Tree.ToggleAscendancyTree(PersistentData.Options.ShowAllAscendancyClasses);
                    break;
                case nameof(Options.TreeComparisonEnabled):
                    UpdateTreeComparison();
                    break;
                case nameof(Options.Theme):
                case nameof(Options.Accent):
                    SetTheme();
                    break;
            }
            SearchUpdate();
        }

        private void Tree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SkillTree.CharClass):
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAscendancySelectionList();
                    ComputationViewModel?.SharedConfiguration.SetCharacterClass(Tree.CharClass);
                    break;
            }
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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
                        ResetTree();
                        break;
                    case Key.E:
                        await DownloadPoeUrlAsync();
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
                        UndoTreeUrlChange();
                        break;
                    case Key.Y:
                        RedoTreeUrlChange();
                        break;
                    case Key.G:
                        ToggleShowSummary();
                        if (_hoveredNode != null && !_hoveredNode.IsRootNode)
                        {
                            GenerateTooltipForNode(_hoveredNode, true);
                        }
                        break;
                    case Key.F:
                        tbSearch.Focus();
                        tbSearch.SelectAll();
                        break;
                }
            }

            if (HighlightByHoverKeys.Any(key => key == e.Key))
            {
                HighlightNodesByHover();
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
            if (SkillTree.SkillTreeRect.Height == 0) // Not yet initialized
                return;

            updateCanvasSize();
        }

        private void updateCanvasSize()
        {
            double aspectRatio = SkillTree.SkillTreeRect.Width / SkillTree.SkillTreeRect.Height;
            if (zbSkillTreeBackground.ActualWidth / zbSkillTreeBackground.ActualHeight > aspectRatio)
            {
                recSkillTree.Height = zbSkillTreeBackground.ActualHeight;
                recSkillTree.Width = aspectRatio * recSkillTree.Height;
            }
            else
            {
                recSkillTree.Width = zbSkillTreeBackground.ActualWidth;
                recSkillTree.Height = recSkillTree.Width / aspectRatio;
            }
            recSkillTree.UpdateLayout();
            _multransform = SkillTree.SkillTreeRect.Size / new Vector2D(recSkillTree.RenderSize.Width, recSkillTree.RenderSize.Height);
            _addtransform = SkillTree.SkillTreeRect.TopLeft;
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
                if (await BuildsControlViewModel.HandleUnsavedBuilds(message, true))
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

        public async Task ScreenShotAsync()
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

                var clipboardBmp = new RenderTargetBitmap((int)xmax, (int)ymax, 96, 96, PixelFormats.Pbgra32);
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
                clipboardBmp.Render(dw);
                clipboardBmp.Freeze();

                //Save image in clipboard
                Clipboard.SetImage(clipboardBmp);

                //Convert renderTargetBitmap to bitmap
                var stream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(clipboardBmp));
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

        public async Task ImportCharacterAsync()
        {
            await this.ShowDialogAsync(_importViewModels.ImportCharacter(ItemAttributes, Tree), new ImportCharacterWindow());
            UpdateUI();
            SetCurrentBuildUrlFromTree();
        }

        public async Task ImportStashAsync()
        {
            await this.ShowDialogAsync(_importViewModels.ImportStash, new ImportStashWindow());
        }

        public async Task RedownloadTreeAssetsAsync()
        {
            var sMessageBoxText = L10n.Message("The existing skill tree data will be deleted. The data will " +
                                               "be downloaded from the official online skill tree and " +
                                               "is from the latest released version of the game.")
                                     + "\n\n" + L10n.Message("Do you want to continue?");

            var rsltMessageBox = await this.ShowQuestionAsync(sMessageBoxText, image: MessageBoxImage.Warning);
            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    var controller = await ExtendedDialogManager.ShowProgressAsync(this, L10n.Message("Downloading skill tree assets ..."), null);
                    controller.Maximum = 1;
                    controller.SetProgress(0);
                    var assetLoader = new AssetLoader(_httpClient, AppData.GetFolder("Data", true), false);
                    try
                    {
                        assetLoader.MoveToBackup();

                        SkillTree.ClearAssets(); //enable recaching of assets
                        Tree = await CreateSkillTreeAsync(controller, assetLoader); //create new skilltree to reinitialize cache
                        recSkillTree.Fill = new VisualBrush(Tree.SkillTreeVisual);

                        await ResetTreeUrl();
                        _justLoaded = false;

                        assetLoader.DeleteBackup();
                    }
                    catch (Exception ex)
                    {
                        assetLoader.RestoreBackup();
                        Log.Error(ex, "Exception while downloading skill tree assets");
                        await this.ShowErrorAsync(L10n.Message("An error occurred while downloading assets."), ex.Message);
                    }
                    await controller.CloseAsync();
                    break;

                case MessageBoxResult.No:
                    //Do nothing
                    break;
            }
        }

        public async Task CheckForUpdatesAsync()
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
                            AppData.ProductName, release.Version)
                          + "\n\n" +
                          L10n.Message("The application will be closed when download completes to proceed with the update.")
                        : string.Format(L10n.Message("A new version {0} is available!"), release.Version)
                          + "\n\n" +
                          L10n.Message(
                              "The new version of application will be installed side-by-side with earlier versions.");

                    if (release.IsPreRelease)
                        message += "\n\n" +
                                   L10n.Message("Warning: This is a pre-release, meaning there could be some bugs!");

                    message += "\n\n" +
                               (release.IsUpdate
                                   ? L10n.Message("Do you want to download and install the update?")
                                   : L10n.Message("Do you want to download and install the new version?"));

                    var download = await this.ShowQuestionAsync(message, title: L10n.Message("Continue installation?"),
                        image: release.IsPreRelease ? MessageBoxImage.Warning : MessageBoxImage.Question);
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
                    if (Updater.LatestReleaseIsUpdate) Application.Current.Shutdown();
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

            var charClass = (CharacterClass) cbCharType.SelectedItem;
            if (Tree.CharClass == charClass) return;

            Tree.SwitchClass(charClass);
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

        private void PopulateAscendancySelectionList()
        {
            if (!Tree.UpdateAscendancyClasses) return;
            Tree.UpdateAscendancyClasses = false;
            var ascendancyItems = new List<string> { "None" };
            foreach (var name in Tree.AscendancyClasses.GetClasses(Tree.CharClass))
                ascendancyItems.Add(name.DisplayName);
            cbAscType.ItemsSource = ascendancyItems.Select(x => new ComboBoxItem { Name = x, Content = x });
        }

        private void Level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> args)
        {
            if (Tree == null)
                return;
            UpdateUI();
        }

        public void ResetTree()
        {
            if (Tree == null)
                return;
            Tree.Reset();
            UpdateUI();
            SetCurrentBuildUrlFromTree();
        }

#endregion

#region Update Attribute lists

        public void UpdateUI()
        {
            UpdateAttributeList();
            RefreshAttributeLists();
            UpdateClass();
            UpdatePoints();
        }

        public void UpdateClass()
        {
            cbCharType.SelectedItem = Tree.CharClass;
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
            NormalUsedPoints.Text = points["NormalUsed"].ToString();
            NormalTotalPoints.Text = points["NormalTotal"].ToString();
            AscendancyUsedPoints.Text = points["AscendancyUsed"].ToString();
            AscendancyTotalPoints.Text = points["AscendancyTotal"].ToString();
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

#region Attribute lists - Event Handlers

        private void ToggleAttributes()
        {
            PersistentData.Options.AttributesBarOpened = !PersistentData.Options.AttributesBarOpened;
        }

        private void ToggleShowSummary()
        {
            PersistentData.Options.ChangeSummaryEnabled = !PersistentData.Options.ChangeSummaryEnabled;
        }

        private void HighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            var listBox = _attributeContextMenu.PlacementTarget as ListBox;
            if (listBox == null || !listBox.IsVisible) return;

            var newHighlightedAttribute =
                "^" + Regex.Replace(listBox.SelectedItem.ToString()!
                        .Replace(@"+", @"\+")
                        .Replace(@"-", @"\-")
                        .Replace(@"%", @"\%"), @"[0-9]*\.?[0-9]+", @"[0-9]*\.?[0-9]+") + "$";
            Tree.HighlightNodesBySearch(newHighlightedAttribute, true, NodeHighlighter.HighlightState.FromAttrib);
        }

        private void UnhighlightNodesByAttribute(object sender, RoutedEventArgs e)
        {
            Tree.HighlightNodesBySearch("", true, NodeHighlighter.HighlightState.FromAttrib);
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
            UpdateAttributeList();
            RefreshAttributeLists();
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
            if (node != null && !node.IsRootNode)
            {
                if (node.IsAscendancyNode && !Tree.DrawAscendancy)
                    return;
                var ascendancyClassName = Tree.AscendancyClassName;
                if (!PersistentData.Options.ShowAllAscendancyClasses && node.IsAscendancyNode && node.AscendancyName != ascendancyClassName)
                    return;
                // Ignore clicks on character portraits and masteries
                if (node.Character == null && node.Type != PassiveNodeType.Mastery)
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
                            _prePath = Tree.GetShortestPathTo(node);
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
            if (node != null && !node.IsRootNode)
            {
                GenerateTooltipForNode(node);
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

        private void zbSkillTreeBackground_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.TwoFingerTap)
                zbSkillTreeBackground.Reset();
        }

        private void GenerateTooltipForNode(SkillNode node, bool forcerefresh = false)
        {
            if (!Tree.DrawAscendancy && node.IsAscendancyNode && !forcerefresh)
                return;
            if (!PersistentData.Options.ShowAllAscendancyClasses && node.IsAscendancyNode &&
                node.AscendancyName != Tree.AscendancyClassName)
                return;

            var socketedJewel = GetSocketedJewel(node);
            if (node.Type == PassiveNodeType.JewelSocket)
            {
                Tree.DrawJewelHighlight(node, socketedJewel);
            }

            if (Tree.SkilledNodes.Contains(node))
            {
                _toRemove = Tree.ForceRefundNodePreview(node);
                if (_toRemove != null)
                    Tree.DrawRefundPreview(_toRemove);
            }
            else
            {
                _prePath = Tree.GetShortestPathTo(node);
                if (node.Type != PassiveNodeType.Mastery)
                    Tree.DrawPath(_prePath);
            }
            var tooltip = node.Name;
            if (node.Attributes.Count != 0)
                tooltip += "\n" + node.StatDefinitions.Aggregate((s1, s2) => s1 + "\n" + s2);
            if (!(_sToolTip.IsOpen && _lasttooltip == tooltip) | forcerefresh)
            {
                _sToolTip.Content = CreateTooltipForNode(node, tooltip, socketedJewel);
                if (!HighlightByHoverKeys.Any(Keyboard.IsKeyDown))
                {
                    _sToolTip.IsOpen = true;
                }
                _lasttooltip = tooltip;
            }
        }

        private Item? GetSocketedJewel(SkillNode node) =>
            node.Type == PassiveNodeType.JewelSocket ? ItemAttributes.GetItemInSlot(ItemSlot.SkillTree, node.Id) : null;

        private object CreateTooltipForNode(SkillNode node, string tooltip, Item? socketedJewel)
        {
            var sp = new StackPanel();

            if (socketedJewel is null)
            {
                sp.Children.Add(new TextBlock {Text = tooltip});
            }
            else
            {
                sp.Children.Add(new ItemTooltip {DataContext = socketedJewel});
                ComputationViewModel!.AttributesInJewelRadius.Calculate(
                    node.Id, socketedJewel.JewelRadius, socketedJewel.ExplicitMods.Select(m => m.Attribute).ToList());
                sp.Children.Add(new AttributesInJewelRadiusView {DataContext = ComputationViewModel.AttributesInJewelRadius});
            }

            if (node.ReminderText != null)
            {
                sp.Children.Add(new Separator());
                sp.Children.Add(new TextBlock {Text = node.ReminderText.Aggregate((s1, s2) => s1 + '\n' + s2)});
            }

            if (_prePath != null && node.Type != PassiveNodeType.Mastery)
            {
                var points = _prePath.Count(n => !n.IsAscendancyStart && !Tree.SkilledNodes.Contains(n));
                sp.Children.Add(new Separator());
                sp.Children.Add(new TextBlock {Text = "Points to skill node: " + points});
            }

            //Change summary, activated with ctrl
            if (PersistentData.Options.ChangeSummaryEnabled)
            {
                //Sum up the total change to attributes and add it to the tooltip
                if (_prePath != null | _toRemove != null)
                {

                    var attributechanges = new Dictionary<string, List<float>>();

                    int changedNodes;

                    if (_prePath != null)
                    {
                        var nodesToAdd = _prePath.Where(n => !Tree.SkilledNodes.Contains(n)).ToList();
                        attributechanges = SkillTree.GetAttributesWithoutImplicitNodesOnly(nodesToAdd);
                        tooltip = "Total gain:";
                        changedNodes = nodesToAdd.Count;
                    }
                    else if (_toRemove != null)
                    {
                        attributechanges = SkillTree.GetAttributesWithoutImplicitNodesOnly(_toRemove);
                        tooltip = "Total loss:";
                        changedNodes = _toRemove.Count;
                    }
                    else
                    {
                        changedNodes = 0;
                    }

                    if (changedNodes > 1)
                    {
                        foreach (var attrchange in attributechanges)
                        {
                            if (attrchange.Value.Count != 0)
                            {
                                var regex = new Regex(Regex.Escape("#"));
                                var attr = attrchange.Key;
                                foreach (var val in attrchange.Value)
                                    attr = regex.Replace(attr, val.ToString(), 1);
                                tooltip += "\n" + attr;
                            }
                        }

                        sp.Children.Add(new Separator());
                        sp.Children.Add(new TextBlock {Text = tooltip});
                    }
                }
            }

            return sp;
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

            _jewelSocketObserver.ResetTreeJewelViewModels();
            _abyssalSocketObserver?.ResetItemJewelViewModels();
            if (ItemAttributes != null)
            {
                ItemAttributes.ItemDataChanged -= ItemAttributesOnItemDataChanged;
                ItemAttributes.Dispose();
            }
            SkillsEditingViewModel?.Dispose();
            SkillTreeAreaViewModel?.Dispose();
            InventoryViewModel?.Dispose();

            var equipmentData = PersistentData.EquipmentData;
            var itemData = PersistentData.CurrentBuild.ItemData;
            var skillDefinitions = await _gameData.Skills;
            ItemAttributes itemAttributes;
            try
            {
                itemAttributes = new ItemAttributes(equipmentData, skillDefinitions, itemData);
            }
            catch (Exception ex)
            {
                itemAttributes = new ItemAttributes(equipmentData, skillDefinitions);
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load item data."),
                    ex.Message);
            }

            itemAttributes.ItemDataChanged += ItemAttributesOnItemDataChanged;
            _equipmentConverter?.ConvertFrom(itemAttributes);
            ItemAttributes = itemAttributes;
            InventoryViewModel =
                new InventoryViewModel(_dialogCoordinator, itemAttributes, await GetJewelPassiveNodesAsync());
            SkillTreeAreaViewModel = new SkillTreeAreaViewModel(SkillTree.Skillnodes, InventoryViewModel.TreeJewels);
            SkillsEditingViewModel = new SkillsEditingViewModel(skillDefinitions, equipmentData.ItemImageService, itemAttributes);
            _abyssalSocketObserver?.SetItemJewelViewModels(InventoryViewModel.ItemJewels);
            Tree.JewelViewModels = InventoryViewModel.TreeJewels;
            UpdateUI();
        }

        private void ItemAttributesOnItemDataChanged(object? sender, EventArgs args)
        {
            _pauseLoadItemData = true;
            PersistentData.CurrentBuild.ItemData = ItemAttributes.ToJsonString();
            _pauseLoadItemData = false;
        }

        private async Task<IEnumerable<ushort>> GetJewelPassiveNodesAsync()
        {
            var treeDefinition = await _gameData.PassiveTree;
            return treeDefinition.Nodes
                .Where(d => d.Type == PassiveNodeType.JewelSocket)
                .Select(d => d.Id);
        }

#endregion

#region Builds - Services

        private async Task CurrentBuildChanged()
        {
            var build = PersistentData.CurrentBuild;
            InputTreeUrl = PersistentData.CurrentBuild.TreeUrl;
            Tree.ResetTaggedNodes();
            TreeGeneratorInteraction?.LoadSettings();
            await LoadItemData();
            SetCustomGroups(build.CustomGroups);
            await ResetTreeUrl();
            _jewelSocketObserver.SetTreeJewelViewModels(InventoryViewModel.TreeJewels);
            ComputationViewModel?.SharedConfiguration.SetBandit(build.Bandits.Choice);
        }

        /// <summary>
        /// Call this to set CurrentBuild.TreeUrl when there were direct SkillTree changes.
        /// </summary>
        private void SetCurrentBuildUrlFromTree()
        {
            _skipLoadOnCurrentBuildTreeChange = true;
            PersistentData.CurrentBuild.TreeUrl = Tree.Serializer.ToUrl();
            _skipLoadOnCurrentBuildTreeChange = false;
        }

        private Task ResetTreeUrl()
        {
            return SetTreeUrl(PersistentData.CurrentBuild.TreeUrl);
        }

        private async Task SetTreeUrl(string treeUrl)
        {
            try
            {
                // If the url did change, it'll run through this method again anyway.
                // So no need to call Tree.LoadFromUrl in that case.
                if (PersistentData.CurrentBuild.TreeUrl == treeUrl)
                    Tree.LoadFromUrl(treeUrl);
                else
                    PersistentData.CurrentBuild.TreeUrl = treeUrl;

                if (_justLoaded)
                {
                    if (_undoList.Count > 1)
                    {
                        var holder = _undoList.Pop();
                        _undoList.Clear();
                        _undoList.Push(holder);
                    }
                }
                else
                {
                    UpdateClass();
                    Tree.UpdateAscendancyClasses = true;
                    PopulateAscendancySelectionList();
                }
                UpdateUI();
                _justLoaded = false;
            }
            catch (Exception ex)
            {
                PersistentData.CurrentBuild.TreeUrl = Tree.Serializer.ToUrl();
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
            }
        }

        private async Task LoadBuildFromUrlAsync(string treeUrl, bool forceBanditsUpdate = false)
        {
            try
            {
                var normalizedUrl = await _buildUrlNormalizer.NormalizeAsync(treeUrl, AwaitAsyncTask);
                BuildUrlData data = SkillTree.DecodeUrl(normalizedUrl, Tree);
                var newTreeUrl = new BuildUrlDataToUrlSerializer(data, SkillTree.Skillnodes.Keys.ToHashSet()).ToUrl();

                BanditSettings bandits = PersistentData.CurrentBuild.Bandits;
                if (forceBanditsUpdate)
                {
                    bandits.Choice = data.Bandit ?? Bandit.None;
                }
                else if (data != null && data.Bandit is Bandit bandit && bandits.Choice != bandit)
                {
                    var details = CreateDetailsString(bandits, data);

                    var dialogResult = await this.ShowQuestionAsync(
                        L10n.Message("The build you are loading contains information about selected bandits.") + Environment.NewLine +
                        L10n.Message("Do you want to use it and overwrite current settings?"),
                        details, L10n.Message("Replace Bandits settings"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        bandits.Choice = bandit;
                    }
                }

                if (data?.CompatibilityIssues != null && data.CompatibilityIssues.Any())
                {
                    await this.ShowWarningAsync(string.Join(Environment.NewLine, data.CompatibilityIssues));
                }

                PersistentData.CurrentBuild.TreeUrl = newTreeUrl;
                InputTreeUrl = newTreeUrl;
            }
            catch (Exception ex)
            {
                PersistentData.CurrentBuild.TreeUrl = Tree.Serializer.ToUrl();
                await this.ShowErrorAsync(L10n.Message("An error occurred while attempting to load Skill tree from URL."), ex.Message);
            }
        }

        private string CreateDetailsString(BanditSettings bandits, BuildUrlData data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(L10n.Message("Current: ")).Append(bandits.Choice)
                .AppendLine(L10n.Message("Loaded: ")).Append(data.Bandit);

            string details = sb.ToString();
            return details;
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

        public void ClearSearch()
        {
            tbSearch.Text = "";
            SearchUpdate();
        }

        private void tbSkillURL_TextChanged(object sender, TextChangedEventArgs e)
        {
            _undoList.Push(PersistentData.CurrentBuild.TreeUrl);
        }

        private bool CanUndoTreeUrlChange() =>
            _undoList.Any(s => s != PersistentData.CurrentBuild.TreeUrl);

        private void UndoTreeUrlChange()
        {
            if (_undoList.Count <= 0) return;
            if (_undoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _undoList.Count > 1)
            {
                _undoList.Pop();
                UndoTreeUrlChange();
            }
            else if (_undoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                _redoList.Push(PersistentData.CurrentBuild.TreeUrl);
                PersistentData.CurrentBuild.TreeUrl = _undoList.Pop();
                UpdateUI();
            }
        }

        private bool CanRedoTreeUrlChange() =>
            _redoList.Any(s => s != PersistentData.CurrentBuild.TreeUrl);

        private void RedoTreeUrlChange()
        {
            if (_redoList.Count <= 0) return;
            if (_redoList.Peek() == PersistentData.CurrentBuild.TreeUrl && _redoList.Count > 1)
            {
                _redoList.Pop();
                RedoTreeUrlChange();
            }
            else if (_redoList.Peek() != PersistentData.CurrentBuild.TreeUrl)
            {
                PersistentData.CurrentBuild.TreeUrl = _redoList.Pop();
                UpdateUI();
            }
        }

        public async Task DownloadPoeUrlAsync()
        {
            var regx =
                new Regex(
                    @"https?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?",
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
                            _httpClient.GetStringAsync("http://poeurl.com/shrink.php?url=" + url));
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

        private void InitializeTheme()
        {
            SetTheme();
            SyncThemeManagers(null, null);
            ThemeManager.IsThemeChanged += SyncThemeManagers;
        }

        private void SetTheme()
        {
            ThemeManager.ChangeTheme(Application.Current, PersistentData.Options.Theme, PersistentData.Options.Accent);
        }

        private void SyncThemeManagers(object? sender, OnThemeChangedEventArgs? args)
        {
            var mahAppsTheme = args?.Theme ?? ThemeManager.DetectTheme();
            Fluent.ThemeManager.ChangeTheme(this, mahAppsTheme.Name);
        }
#endregion

        private void UpdateTreeComparison()
        {
            if (Tree == null)
                return;

            var build = PersistentData.SelectedBuild as PoEBuild;
            if (build != null && PersistentData.Options.TreeComparisonEnabled)
            {
                SkillTree.DecodeUrl(build.TreeUrl, out var nodes, out var charClass, Tree);

                Tree.HighlightedNodes.Clear();
                Tree.HighlightedNodes.UnionWith(nodes);
                Tree.HighlightedAttributes = SkillTree.GetAttributes(nodes, charClass, build.Level, build.Bandits);
            }
            else
            {
                Tree.HighlightedNodes.Clear();
                Tree.HighlightedAttributes = null;
            }
            UpdateUI();
        }

        public async Task CraftItemAsync()
        {
            await CraftItemAsync(new CraftingViewModel(PersistentData.EquipmentData), new CraftingView());
        }

        public async Task CraftUniqueAsync()
        {
            await CraftItemAsync(new UniqueCraftingViewModel(PersistentData.EquipmentData), new UniqueCraftingView());
        }

        private async Task CraftItemAsync<TBase>(AbstractCraftingViewModel<TBase> viewModel, BaseDialog view)
            where TBase: class, IItemBase
        {
            if (!await this.ShowDialogAsync(viewModel, view))
            {
                return;
            }

            var item = viewModel.Item;
            if (StashViewModel.Items.Count > 0)
            {
                item.Y = StashViewModel.LastOccupiedRow + 1;
            }

            StashViewModel.AddItem(item, true);
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
            foreach (var property in appSettings.Properties.Cast<SettingsProperty>())
            {
                property.Provider = provider;
            }
            appSettings.Reload();
            return settings;
        }
    }
}