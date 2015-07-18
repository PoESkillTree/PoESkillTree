using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Settings
{
    public class AutomatedSolverSettings : SolverSettings
    {
        public AutomatedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed, Dictionary<string, float> initialStats, HashSet<ushort> subsetTree, HashSet<ushort> initialTree)
            : base(level, totalPoints, @checked, crossed, initialStats, subsetTree, initialTree)
        {
        }
    }
}