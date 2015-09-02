using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Settings
{
    public enum CombinedStat
    {
        TotalHealth,
        DpsMultiplier,
        MainSkillDps
    }

    public class AdvancedSolverSettings : SolverSettings
    {
        // Weights are a value between 0 and 1. Tuple<target, weight>
        public readonly Dictionary<string, Tuple<float, double>> AttributeConstraints;

        public readonly Dictionary<CombinedStat, Tuple<float, double>> CombinedConstraints;

        /// <summary>
        /// Starting attributes of stats that calculations are based on. (e.g. base attributes, attributes from items)
        /// </summary>
        public readonly Dictionary<string, float> InitialAttributes;

        public AdvancedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
            HashSet<ushort> subsetTree, HashSet<ushort> initialTree, Dictionary<string, float> initialAttributes,
            Dictionary<string, Tuple<float, double>> attributeConstraints, Dictionary<CombinedStat, Tuple<float, double>> combinedConstraints)
            : base(level, totalPoints, @checked, crossed, subsetTree, initialTree)
        {
            AttributeConstraints = attributeConstraints ?? new Dictionary<string, Tuple<float, double>>();
            CombinedConstraints = combinedConstraints ?? new Dictionary<CombinedStat, Tuple<float, double>>();
            InitialAttributes = initialAttributes ?? new Dictionary<string, float>();

            if (AttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "attributeConstraints");
            }

            if (CombinedConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "combinedConstraints");
            }
        }

        public AdvancedSolverSettings(SolverSettings baseSettings,
            Dictionary<string, float> initialAttributes,
            Dictionary<string, Tuple<float, double>> attributeConstraints,
            Dictionary<CombinedStat, Tuple<float, double>> combinedConstraints)
            : this(baseSettings.Level, baseSettings.TotalPoints, baseSettings.Checked, baseSettings.Crossed,
                baseSettings.SubsetTree, baseSettings.InitialTree, initialAttributes,
                attributeConstraints, combinedConstraints)
        {
        }
    }
}