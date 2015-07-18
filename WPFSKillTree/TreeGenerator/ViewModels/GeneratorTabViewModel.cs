using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public abstract class GeneratorTabViewModel : ViewModelBase
    {
        protected readonly SkillTree Tree;

        protected GeneratorTabViewModel(SkillTree tree)
        {
            Tree = tree;
        }

        public abstract ISolver CreateSolver(SolverSettings settings);
    }
}