using System;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    /// <summary>
    /// Base class for tabs in SettingsViewModel that specify which solver
    /// to use and offer settings to customize the solver execution.
    /// </summary>
    public abstract class GeneratorTabViewModel : AbstractCompositeSetting
    {
        private string _displayName;
        /// <summary>
        /// Returns the user-friendly name of this object.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            protected set { SetProperty(ref _displayName, value); }
        }

        /// <summary>
        /// Gets the <see cref="IDialogCoordinator"/> used to display dialogs.
        /// </summary>
        protected IDialogCoordinator DialogCoordinator { get; }

        /// <summary>
        /// Gets the context used for <see name="DialogCoordinator"/>.
        /// </summary>
        protected object DialogContext { get; }

        /// <summary>
        /// The number of iterations this solver will run.
        /// </summary>
        public LeafSetting<int> Iterations { get; }

        /// <summary>
        /// Whether checked nodes need to be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> IncludeChecked { get; }

        /// <summary>
        /// Whether crossed nodes must not be skilled by the solver.
        /// </summary>
        public LeafSetting<bool> ExcludeCrossed { get; }

        /// <summary>
        /// The SkillTree instance to operate on.
        /// </summary>
        public SkillTree Tree { get; }

        /// <summary>
        /// Executes this generator.
        /// </summary>
        public ICommand RunCommand { get; }

        /// <summary>
        /// Instantiates a new GeneratorTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator"/> used to display dialogs.</param>
        /// <param name="dialogContext">The context used for <paramref name="dialogCoordinator"/>.</param>
        /// <param name="defaultIterations">The default value for <see cref="Iterations"/>.</param>
        /// <param name="runCallback">The action that is called when <see cref="RunCommand"/> is executed.</param>
        protected GeneratorTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator, object dialogContext,
            int defaultIterations, Action<GeneratorTabViewModel> runCallback)
        {
            Tree = tree;
            DialogCoordinator = dialogCoordinator;
            DialogContext = dialogContext;
            Iterations = new LeafSetting<int>(nameof(Iterations), defaultIterations);
            IncludeChecked = new LeafSetting<bool>(nameof(IncludeChecked), true);
            ExcludeCrossed = new LeafSetting<bool>(nameof(ExcludeCrossed), true);

            RunCommand = new RelayCommand(() => runCallback(this));
        }

        public Task<ISolver> CreateSolverAsync()
        {
            var @checked = IncludeChecked.Value ? Tree.GetCheckedNodes() : null;
            var crossed = ExcludeCrossed.Value ? Tree.GetCrossedNodes() : null;
            var iterations = Iterations.Value;
            var settings = new SolverSettings(@checked, crossed, iterations);
            return CreateSolverAsync(settings);
        }

        /// <summary>
        /// Creates a solver that uses the settings defined by the user in this ViewModel.
        /// </summary>
        /// <param name="settings">(not null) Base settings specified in GeneratorTabViewModel.</param>
        protected abstract Task<ISolver> CreateSolverAsync(SolverSettings settings);
    }
}