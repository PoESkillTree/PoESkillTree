using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;

namespace POESKillTree.TreeGenerator.Settings
{
    public class AdvancedSolverSettings : SolverSettings
    {
        // Weights are a value between 0 and 1. Tuple<target, weight>
        public readonly Dictionary<string, Tuple<float, double>> AttributeConstraints;

        public readonly Dictionary<PseudoAttribute, Tuple<float, double>> PseudoAttributeConstraints;

        public readonly WeaponClass WeaponClass;

        public readonly Tags Tags;

        public readonly OffHand OffHand;

        /// <summary>
        /// Starting attributes of stats that calculations are based on. (e.g. base attributes, attributes from items)
        /// </summary>
        public readonly Dictionary<string, float> InitialAttributes;

        public AdvancedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
            HashSet<ushort> subsetTree, HashSet<ushort> initialTree, Dictionary<string, float> initialAttributes,
            Dictionary<string, Tuple<float, double>> attributeConstraints, Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints,
            WeaponClass weaponClass, Tags tags, OffHand offHand)
            : base(level, totalPoints, @checked, crossed, subsetTree, initialTree)
        {
            WeaponClass = weaponClass;
            Tags = tags;
            OffHand = offHand;
            AttributeConstraints = attributeConstraints ?? new Dictionary<string, Tuple<float, double>>();
            PseudoAttributeConstraints = pseudoAttributeConstraints ?? new Dictionary<PseudoAttribute, Tuple<float, double>>();
            InitialAttributes = initialAttributes ?? new Dictionary<string, float>();

            if (AttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "attributeConstraints");
            }

            if (PseudoAttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
            {
                throw new ArgumentException("Weights need to be between 0 and 1", "pseudoAttributeConstraints");
            }
        }

        public AdvancedSolverSettings(SolverSettings baseSettings,
            Dictionary<string, float> initialAttributes,
            Dictionary<string, Tuple<float, double>> attributeConstraints,
            Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints,
            WeaponClass weaponClass, Tags tags, OffHand offHand)
            : this(baseSettings.Level, baseSettings.TotalPoints, baseSettings.Checked, baseSettings.Crossed,
                baseSettings.SubsetTree, baseSettings.InitialTree, initialAttributes,
                attributeConstraints, pseudoAttributeConstraints, weaponClass, tags, offHand)
        {
        }
    }
}