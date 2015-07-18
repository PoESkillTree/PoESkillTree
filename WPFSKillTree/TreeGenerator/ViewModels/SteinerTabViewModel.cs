using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class SteinerTabViewModel : GeneratorTabViewModel
    {

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