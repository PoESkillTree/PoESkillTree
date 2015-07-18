using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AutomatedTabViewModel : GeneratorTabViewModel
    {
        public AutomatedTabViewModel(SkillTree tree)
            : base(tree)
        {
            DisplayName = L10n.Message("Automated");
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}