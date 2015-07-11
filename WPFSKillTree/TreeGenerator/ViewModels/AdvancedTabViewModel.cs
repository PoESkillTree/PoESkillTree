using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {
        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            DisplayName = L10n.Message("Advanced");
        }

        public override AbstractSolver<SolverSettings> CreateSolver(SolverSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}