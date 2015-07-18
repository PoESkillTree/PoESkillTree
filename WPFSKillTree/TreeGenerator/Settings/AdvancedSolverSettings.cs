using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Settings
{
    public enum CombinedStat
    {
        // TODO fill up
        TotalHealth,
        DpsMultiplier,
        MainSkillDps
    }

    public class AdvancedSolverSettings : SolverSettings
    {
        // Weights are a value between 0 and 1. Tuple<target, weight>
        public readonly Dictionary<string, Tuple<float, double>> StatConstraints;

        public readonly Dictionary<CombinedStat, Tuple<float, double>> CombinedConstraints;

        public AdvancedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
            Dictionary<string, float> initialStats, HashSet<ushort> subsetTree, HashSet<ushort> initialTree,
            Dictionary<string, Tuple<float, double>> statConstraints, Dictionary<CombinedStat, Tuple<float, double>> combinedConstraints)
            : base(level, totalPoints, @checked, crossed, initialStats, subsetTree, initialTree)
        {
            StatConstraints = statConstraints ?? new Dictionary<string, Tuple<float, double>>();
            CombinedConstraints = combinedConstraints ?? new Dictionary<CombinedStat, Tuple<float, double>>();

            if (StatConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "statConstraints");
            }

            if (CombinedConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "combinedConstraints");
            }
        }

        public AdvancedSolverSettings(SolverSettings baseSettings,
            Dictionary<string, Tuple<float, double>> statConstraints,
            Dictionary<CombinedStat, Tuple<float, double>> combinedConstraints)
            : this(baseSettings.Level, baseSettings.TotalPoints, baseSettings.Checked, baseSettings.Crossed,
                baseSettings.InitialStats, baseSettings.SubsetTree, baseSettings.InitialTree,
                statConstraints, combinedConstraints)
        {
        }
    }
}