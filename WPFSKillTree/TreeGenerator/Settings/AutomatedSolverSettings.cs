using System.Collections.Generic;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.TreeGenerator.Settings
{
    /// <summary>
    /// Data class for settings for AutomatedSolver.
    /// </summary>
    public class AutomatedSolverSettings : SolverSettings
    {
        public AutomatedSolverSettings(int level, int totalPoints, ISet<SkillNode> @checked,
            ISet<SkillNode> crossed, ISet<SkillNode> subsetTree, ISet<SkillNode> initialTree, int iterations)
            : base(level, totalPoints, @checked, crossed, subsetTree, initialTree, iterations)
        {
        }
    }
}