using System.Collections.Generic;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Settings
{
    /// <summary>
    /// Data class for settings for AutomatedSolver.
    /// </summary>
    public class AutomatedSolverSettings : SolverSettings
    {
        public AutomatedSolverSettings(int level, int totalPoints, HashSet<SkillNode> @checked,
            HashSet<SkillNode> crossed, ObservableSet<SkillNode> subsetTree, ObservableSet<SkillNode> initialTree, int iterations)
            : base(level, totalPoints, @checked, crossed, subsetTree, initialTree, iterations)
        {
        }
    }
}