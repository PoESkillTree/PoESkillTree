using POESKillTree.Localization;
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

        /// <summary>
        /// Instantiates a new SteinerTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        public SteinerTabViewModel(SkillTree tree)
            : base(tree)
        {
            DisplayName = L10n.Message("Tagged Nodes");
        }

        public override void Reset()
        {
            // nothing to reset
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            return new SteinerSolver(Tree, settings);
            //return new SteinerSmtSolver(Tree, settings);
        }
    }
}