using System.Collections.Generic;
using POESKillTree.Localization;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// GeneratorTabViewModel for skilling tagged nodes with SteinerSolver.
    /// Nothing special done here.
    /// </summary>
    public sealed class SteinerTabViewModel : GeneratorTabViewModel
    {
        protected override string Key { get; } = "SteinerTab";

        protected override IReadOnlyList<ISetting> SubSettings { get; } = new ISetting[0];

        /// <summary>
        /// Instantiates a new SteinerTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        public SteinerTabViewModel(SkillTree tree)
            : base(tree)
        {
            DisplayName = L10n.Message("Tagged Nodes");
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            return new SteinerSolver(Tree, settings);
        }
    }
}