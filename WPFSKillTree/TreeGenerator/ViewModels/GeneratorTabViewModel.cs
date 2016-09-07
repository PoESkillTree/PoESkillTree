using System;
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
        /// The SkillTree instance to operate on.
        /// </summary>
        protected readonly SkillTree Tree;

        /// <summary>
        /// Instantiates a new GeneratorTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        protected GeneratorTabViewModel(SkillTree tree)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            Tree = tree;
        }

        /// <summary>
        /// Creates a solver that uses the settings defined by the user in this ViewModel.
        /// </summary>
        /// <param name="settings">(not null) Base settings specified in SettingsViewModel.</param>
        /// <returns></returns>
        public abstract ISolver CreateSolver(SolverSettings settings);
    }
}