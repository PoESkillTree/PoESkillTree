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

        private SkillTree Tree { get; }

        /// <summary>
        /// Gets or sets the observable collection of <see cref="GeneratorTabViewModel"/> contained in
        /// this ViewModel.
        /// </summary>
        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

        protected override string Key { get; } = "TreeGenerator";

        protected override IReadOnlyList<ISetting> SubSettings { get; }

        #region Presentation

        /// <summary>
        /// Whether checked nodes need to be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> IncludeChecked { get; }

        /// <summary>
        /// Whether crossed nodes must not be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> ExcludeCrossed { get; }

        /// <summary>
        /// The currently selected <see cref="GeneratorTabViewModel"/> which will
        /// provide the solver once <see cref="RunCommand"/> is executed.
        /// </summary>
        public LeafSetting<int> SelectedTabIndex { get; }

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

            IncludeChecked = new LeafSetting<bool>(nameof(IncludeChecked), true);
            ExcludeCrossed = new LeafSetting<bool>(nameof(ExcludeCrossed), true);
            SelectedTabIndex = new LeafSetting<int>(nameof(SelectedTabIndex), 0);

            if (generator == null)
            {
                CreateTabs();
            }
            else
            {
                Tabs = new ObservableCollection<GeneratorTabViewModel> { generator };
            }
            SubSettings = new ISetting[]
            {
                IncludeChecked, ExcludeCrossed, SelectedTabIndex
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
            var @checked = IncludeChecked.Value ? Tree.GetCheckedNodes() : null;
            var crossed = ExcludeCrossed.Value ? Tree.GetCrossedNodes() : null;
            var iterations = Tabs[SelectedTabIndex.Value].Iterations.Value;
            return new SolverSettings(@checked, crossed, iterations);
        }

        /// <summary>
        /// Event raised when <see cref="RunCommand"/> execution is finished
        /// and the skill tree may have changed.
        /// </summary>
        public event EventHandler RunFinished;
    }
}