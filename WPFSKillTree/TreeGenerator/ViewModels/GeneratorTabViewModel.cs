using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
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
        /// The number of iterations this solver will run.
        /// </summary>
        public LeafSetting<int> Iterations { get; }

        /// <summary>
        /// The SkillTree instance to operate on.
        /// </summary>
        public SkillTree Tree { get; }

        /// <summary>
        /// Instantiates a new GeneratorTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator"/> used to display dialogs.</param>
        /// <param name="defaultIterations">The default value for <see cref="Iterations"/>.</param>
        protected GeneratorTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator,
            int defaultIterations)
        {
            Tree = tree;
            DialogCoordinator = dialogCoordinator;
            Iterations = new LeafSetting<int>(nameof(Iterations), defaultIterations);
        }

        /// <summary>
        /// Creates a solver that uses the settings defined by the user in this ViewModel.
        /// </summary>
        /// <param name="settings">(not null) Base settings specified in SettingsViewModel.</param>
        public abstract ISolver CreateSolver(SolverSettings settings);
    }
}