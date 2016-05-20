using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// ViewModel that enables setting up and running <see cref="Solver.ISolver"/> through
    /// contained <see cref="GeneratorTabViewModel"/>s.
    /// </summary>
    public sealed class SettingsViewModel : ViewModelBase
    {

        private readonly SkillTree _tree;

        private readonly ISettingsDialogCoordinator _dialogCoordinator;

        public SkillTree Tree { get { return _tree; } }

        /// <summary>
        /// Gets or sets the observable collection of <see cref="GeneratorTabViewModel"/> contained in
        /// this ViewModel.
        /// </summary>
        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

#region Presentation

        // Default values for the properties to use on construction and on reset.
        private const int AdditionalPointsDefaultValue = 21;
        private const bool IncludeCheckedDefaultValue = true;
        private const bool ExcludeCrossedDefaultValue = true;
        private const bool TreeAsSubsetDefaultValue = false;
        private const bool TreeAsInitialDefaultValue = false;

        private int _additionalPoints = -1;
        /// <summary>
        /// Gets or sets the number of points on top of those provided by level that
        /// the solver can use.
        /// </summary>
        public int AdditionalPoints
        {
            get { return _additionalPoints; }
            set
            {
                SetProperty(ref _additionalPoints, value,
                    () => TotalPoints = _tree.Level - 1 + _additionalPoints);
            }
        }

        private int _totalPoints;
        /// <summary>
        /// Gets or sets total number of points the solver can use.
        /// Equals <see cref="SkillTree.Level"/> - 1 + <see cref="AdditionalPoints"/>.
        /// </summary>
        public int TotalPoints
        {
            get { return _totalPoints; }
            private set { SetProperty(ref _totalPoints, value); }
        }

        private bool _includeChecked = IncludeCheckedDefaultValue;
        /// <summary>
        /// Gets or sets whether checked nodes need to be skilled by the solver.
        /// </summary>
        public bool IncludeChecked
        {
            get { return _includeChecked; }
            set { SetProperty(ref _includeChecked, value); }
        }

        private bool _excludeCrossed = ExcludeCrossedDefaultValue;
        /// <summary>
        /// Gets or sets whether crossed nodes must not be skilled by the solver.
        /// </summary>
        public bool ExcludeCrossed
        {
            get { return _excludeCrossed; }
            set { SetProperty(ref _excludeCrossed, value); }
        }

        private bool _treeAsSubset = TreeAsSubsetDefaultValue;
        /// <summary>
        /// Gets or set whether the nodes skilled by the solver need to be
        /// a subset of the currently skilled nodes.
        /// If <see cref="TreeAsSubset"/> and <see cref="TreeAsInitial"/> are false,
        /// the nodes currently skilled will stay skilled in the solution given by the solver.
        /// </summary>
        public bool TreeAsSubset
        {
            get { return _treeAsSubset; }
            set { SetProperty(ref _treeAsSubset, value); }
        }

        private bool _treeAsInitial = TreeAsInitialDefaultValue;
        /// <summary>
        /// Gets or sets whether the currently skilled nodes should be provided
        /// to the solver as an initial solution.
        /// If <see cref="TreeAsSubset"/> and <see cref="TreeAsInitial"/> are false,
        /// the nodes currently skilled will stay skilled in the solution given by the solver.
        /// </summary>
        public bool TreeAsInitial
        {
            get { return _treeAsInitial; }
            set { SetProperty(ref _treeAsInitial, value); }
        }

        private int _selectedTabIndex;
        /// <summary>
        /// Gets or sets the currently selected <see cref="GeneratorTabViewModel"/> which will
        /// be provide the solver once <see cref="RunCommand"/> is executed.
        /// </summary>
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { SetProperty(ref _selectedTabIndex, value); }
        }

        private int _iterations = 1;
        /// <summary>
        /// Gets or sets number of iterations this solver will run.
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
            set { SetProperty(ref _iterations, value);}
        }

        #endregion

#region Commands

        private RelayCommand _runCommand;
        /// <summary>
        /// Starts a <see cref="ControllerViewModel"/> with the solver returned by the
        /// <see cref="GeneratorTabViewModel"/> at the currently selected <see cref="Tabs"/> index
        /// and sets its result in the skill tree.
        /// </summary>
        public ICommand RunCommand
        {
            get { return _runCommand ?? (_runCommand = new RelayCommand(async o => await RunAsync())); }
        }

        private RelayCommand _resetCommand;
        /// <summary>
        /// Resets all Properties to the values they had on construction.
        /// Calls <see cref="GeneratorTabViewModel.Reset"/> on all tabs.
        /// </summary>
        public ICommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new RelayCommand(o => Reset()));}
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
            if (tree == null) throw new ArgumentNullException("tree");

            DisplayName = L10n.Message("Skill tree Generator").ToUpper();

            _tree = tree;
            _dialogCoordinator = dialogCoordinator;
            AdditionalPoints = CalculateAdditionalPointsNeeded(tree);
            
            tree.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Level")
                {
                    TotalPoints = _tree.Level - 1 + _additionalPoints;
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
        }

        private static int CalculateAdditionalPointsNeeded(SkillTree tree)
        {
            if (tree.Level != SkillTree.UndefinedLevel && tree.SkilledNodes.Count > 1
                && tree.SkilledNodes.Count - tree.Level >= 0)
            {
                return tree.SkilledNodes.Count - tree.Level;
            }
            return AdditionalPointsDefaultValue;
        }

        private void CreateTabs()
        {
            Tabs = new ObservableCollection<GeneratorTabViewModel>
            {
                new AdvancedTabViewModel(_tree),
                new AutomatedTabViewModel(_tree),
                new SteinerTabViewModel(_tree)
            };
        }

        public async Task RunAsync()
        {
            var savedHighlights = _tree.HighlightedNodes;

            var settings = CreateSettings();
            var solver = Tabs[_selectedTabIndex].CreateSolver(settings);

            var controllerResult = await _dialogCoordinator
                .ShowControllerDialogAsync(this, solver, Tabs[_selectedTabIndex].DisplayName, _tree);
            if (controllerResult != null)
            {
                _tree.SkilledNodes = new HashSet<ushort>(controllerResult);
            }
            _tree.HighlightedNodes = savedHighlights;
            _tree.DrawTreeComparisonHighlight();
            _tree.DrawHighlights();
            _tree.UpdateAvailNodes();

            RunFinished.Raise(this);
        }

        private void Reset()
        {
            AdditionalPoints = CalculateAdditionalPointsNeeded(_tree);
            IncludeChecked = IncludeCheckedDefaultValue;
            ExcludeCrossed = ExcludeCrossedDefaultValue;
            TreeAsSubset = TreeAsSubsetDefaultValue;
            TreeAsInitial = TreeAsInitialDefaultValue;
            foreach (var tab in Tabs)
            {
                tab.Reset();
            }
        }

        private SolverSettings CreateSettings()
        {
            var level = Tree.Level;
            var totalPoints = _totalPoints;
            var @checked = _includeChecked ? _tree.GetCheckedNodes() : null;
            var crossed = _excludeCrossed ? _tree.GetCrossedNodes() : null;
            var subsetTree = _treeAsSubset ? _tree.SkilledNodes : null;
            var initialTree = _treeAsInitial ? _tree.SkilledNodes : null;
            var iterations = _iterations;
            return new SolverSettings(level, totalPoints, @checked, crossed, subsetTree, initialTree, iterations);
        }

        /// <summary>
        /// Event raised when <see cref="RunCommand"/> execution is finished
        /// and the skill tree may have changed.
        /// </summary>
        public event EventHandler RunFinished;
    }
}