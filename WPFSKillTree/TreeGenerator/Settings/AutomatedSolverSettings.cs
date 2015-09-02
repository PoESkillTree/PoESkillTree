using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Settings
{
    public class AutomatedSolverSettings : SolverSettings
    {
        public AutomatedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed, HashSet<ushort> subsetTree, HashSet<ushort> initialTree)
            : base(level, totalPoints, @checked, crossed, subsetTree, initialTree)
        {
        }
    }
}