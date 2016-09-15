using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// ViewModel that enables setting up and running <see cref="Solver.ISolver"/> through
    /// contained <see cref="GeneratorTabViewModel"/>s.
    /// </summary>
    public sealed class SettingsViewModel : AbstractCompositeSetting
    {
        private readonly ISettingsDialogCoordinator _dialogCoordinator;

        public SkillTree Tree { get; }

        /// <summary>
        /// Gets or sets the observable collection of <see cref="GeneratorTabViewModel"/> contained in
        /// this ViewModel.
        /// </summary>
        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

        protected override string Key { get; } = "TreeGenerator";

        protected override IReadOnlyList<ISetting> SubSettings { get; }

        #region Presentation

        private int _totalPoints;
        private GeneratorTabViewModel _selectedTab;

        /// <summary>
        /// The number of points on top of those provided by level that
        /// the solver can use.
        /// </summary>
        public LeafSetting<int> AdditionalPoints { get; }

        /// <summary>
        /// Gets the total number of points the solver can use.
        /// Equals <see cref="SkillTree.Level"/> - 1 + <see cref="AdditionalPoints"/>.
        /// </summary>
        public int TotalPoints
        {
            get { return _totalPoints; }
            private set { SetProperty(ref _totalPoints, value); }
        }

        /// <summary>
        /// Whether checked nodes need to be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> IncludeChecked { get; }

        /// <summary>
        /// Whether crossed nodes must not be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> ExcludeCrossed { get; }

        /// <summary>
        /// Whether the nodes skilled by the solver need to be
        /// a subset of the currently skilled nodes.
        /// If <see cref="TreeAsSubset"/> and <see cref="TreeAsInitial"/> are false,
        /// the nodes currently skilled will stay skilled in the solution given by the solver.
        /// </summary>
        public LeafSetting<bool> TreeAsSubset { get; }

        /// <summary>
        /// Whether the currently skilled nodes should be provided
        /// to the solver as an initial solution.
        /// If <see cref="TreeAsSubset"/> and <see cref="TreeAsInitial"/> are false,
        /// the nodes currently skilled will stay skilled in the solution given by the solver.
        /// </summary>
        public LeafSetting<bool> TreeAsInitial { get; }

        /// <summary>
        /// The currently selected <see cref="GeneratorTabViewModel"/> which will
        /// provide the solver once <see cref="RunCommand"/> is executed.
        /// </summary>
        public LeafSetting<int> SelectedTabIndex { get; }

        public GeneratorTabViewModel SelectedTab
        {
            get { return _selectedTab; }
            private set { SetProperty(ref _selectedTab, value); }
        }

        /// <summary>
        /// The number of iterations this solver will run.
        /// </summary>
        public LeafSetting<int> Iterations { get; }

        #endregion

        #region Commands

        private ICommand _runCommand;
        /// <summary>
        /// Starts a <see cref="ControllerViewModel"/> with the solver returned by the
        /// <see cref="GeneratorTabViewModel"/> at the currently selected <see cref="Tabs"/> index
        /// and sets its result in the skill tree.
        /// </summary>
        public ICommand RunCommand
        {
            get { return _runCommand ?? (_runCommand = new AsyncRelayCommand(RunAsync)); }
        }

        private RelayCommand _resetCommand;
        /// <summary>
        /// Resets all Properties to the values they had on construction.
        /// Calls <see cref="GeneratorTabViewModel.Reset"/> on all tabs.
        /// </summary>
        public ICommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new RelayCommand(Reset));}
        }

        #endregion

        /// <summary>
        /// Constructs a new SettingsViewModel that operates on the given skill tree.
        /// </summary>
        /// <param name="tree">The skill tree to operate on. (not null)</param>
        /// <param name="dialogCoordinator"></param>
        /// <param name="generator">Optional <see cref="GeneratorTabViewModel"/> initialize
        /// <see cref="Tabs"/> with. If non is provided, <see cref="AdvancedTabViewModel"/>,
        /// <see cref="AutomatedTabViewModel"/> and <see cref="SteinerTabViewModel"/> will be
        /// added to <see cref="Tabs"/>.</param>
        public SettingsViewModel(SkillTree tree, ISettingsDialogCoordinator dialogCoordinator, GeneratorTabViewModel generator = null)
        {
            Tree = tree;
            _dialogCoordinator = dialogCoordinator;

            AdditionalPoints = new LeafSetting<int>(nameof(AdditionalPoints), 21,
                () => TotalPoints = Tree.Level - 1 + AdditionalPoints.Value);
            TotalPoints = Tree.Level - 1 + AdditionalPoints.Value;
            IncludeChecked = new LeafSetting<bool>(nameof(IncludeChecked), true);
            ExcludeCrossed = new LeafSetting<bool>(nameof(ExcludeCrossed), true);
            TreeAsSubset = new LeafSetting<bool>(nameof(TreeAsSubset), false);
            TreeAsInitial = new LeafSetting<bool>(nameof(TreeAsInitial), false);
            SelectedTabIndex = new LeafSetting<int>(nameof(SelectedTabIndex), 0,
                () => SelectedTab = Tabs[SelectedTabIndex.Value]);
            Iterations = new LeafSetting<int>(nameof(Iterations), 3);

            tree.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SkillTree.Level))
                {
                    TotalPoints = Tree.Level - 1 + AdditionalPoints.Value;
                }
            };

            if (generator == null)
            {
                CreateTabs();
            }
            else
            {
                Tabs = new ObservableCollection<GeneratorTabViewModel> { generator };
            }
            SelectedTab = Tabs[SelectedTabIndex.Value];
            SubSettings = new ISetting[]
            {
                AdditionalPoints, IncludeChecked, ExcludeCrossed, TreeAsSubset, TreeAsInitial,
                SelectedTabIndex, Iterations
            }.Union(Tabs).ToArray();
        }

        private void CreateTabs()
        {
            Tabs = new ObservableCollection<GeneratorTabViewModel>
            {
                new SteinerTabViewModel(Tree, _dialogCoordinator),
                new AdvancedTabViewModel(Tree, _dialogCoordinator),
                new AutomatedTabViewModel(Tree, _dialogCoordinator)
            };
        }

        private async Task RunAsync()
        {
            var savedHighlights = Tree.HighlightedNodes.ToList();

            var settings = CreateSettings();
            var solver = Tabs[SelectedTabIndex.Value].CreateSolver(settings);
            if (solver == null)
                return;

            var controllerResult = await _dialogCoordinator
                .ShowControllerDialogAsync(this, solver, Tabs[SelectedTabIndex.Value].DisplayName, Tree);
            if (controllerResult != null)
            {
                Tree.SkilledNodes.Clear();
                Tree.AllocateSkillNodes(controllerResult.Select(n => SkillTree.Skillnodes[n]));
            }
            Tree.HighlightedNodes.Clear();
            Tree.HighlightedNodes.UnionWith(savedHighlights);
            Tree.DrawHighlights();

            RunFinished?.Invoke(this, EventArgs.Empty);
        }

        private SolverSettings CreateSettings()
        {
            var level = Tree.Level;
            var totalPoints = _totalPoints;
            var @checked = IncludeChecked.Value ? Tree.GetCheckedNodes() : null;
            var crossed = ExcludeCrossed.Value ? Tree.GetCrossedNodes() : null;
            var subsetTree = TreeAsSubset.Value ? Tree.SkilledNodes : null;
            var initialTree = TreeAsInitial.Value ? Tree.SkilledNodes : null;
            var iterations = Iterations.Value;
            return new SolverSettings(level, totalPoints, @checked, crossed, subsetTree, initialTree, iterations);
        }

        /// <summary>
        /// Event raised when <see cref="RunCommand"/> execution is finished
        /// and the skill tree may have changed.
        /// </summary>
        public event EventHandler RunFinished;
    }
}