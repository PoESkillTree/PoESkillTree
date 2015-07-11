using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Settings
{
    public class AdvancedSolverSettings : SolverSettings
    {
        public AdvancedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed, Dictionary<string, List<float>> initialStats, HashSet<ushort> subsetTree, HashSet<ushort> initialTree) : base(level, totalPoints, @checked, crossed, initialStats, subsetTree, initialTree)
        {
        }
    }
}