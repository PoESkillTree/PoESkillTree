using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;

namespace POESKillTree.TreeGenerator.Settings
{
    /// <summary>
    /// Data class for settings for AdvancedSolver.
    /// </summary>
    public class AdvancedSolverSettings : SolverSettings
    {
        /// <summary>
        /// The attribute constraints the solver should try to fullfill.
        /// The key is the name of the attribute.
        /// Value is a tuple of target value (float) and weight (double).
        /// Weight must be between 0 and 1 (both inclusive).
        /// </summary>
        public readonly Dictionary<string, Tuple<float, double>> AttributeConstraints;

        /// <summary>
        /// The pseudo attribute constraints the solver should try to fullfill.
        /// The key is the name of the attribute.
        /// Value is a tuple of target value (float) and weight (double).
        /// Weight must be between 0 and 1 (both inclusive).
        /// </summary>
        public readonly Dictionary<PseudoAttribute, Tuple<float, double>> PseudoAttributeConstraints;

        /// <summary>
        /// WeaponClass used for pseudo attribute calculation.
        /// </summary>
        public readonly WeaponClass WeaponClass;

        /// <summary>
        /// Tags used for pseudo attribute calculation.
        /// </summary>
        public readonly Tags Tags;

        /// <summary>
        /// OffHand used for pseudo attribute calculation.
        /// </summary>
        public readonly OffHand OffHand;

        /// <summary>
        /// Starting attributes of stats that calculations are based on.
        /// (e.g. base attributes, attributes from items)
        /// </summary>
        public readonly Dictionary<string, float> InitialAttributes;

        private AdvancedSolverSettings(int level, int totalPoints, HashSet<ushort> @checked, HashSet<ushort> crossed,
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
                throw new ArgumentException("Weights need to be between 0 and 1", "attributeConstraints");
            if (AttributeConstraints.Values.Any(t => t.Item1 <= 0))
                throw new ArgumentException("Target values need to be greater zero", "attributeConstraints");
            if (PseudoAttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
                throw new ArgumentException("Weights need to be between 0 and 1", "pseudoAttributeConstraints");
            if (PseudoAttributeConstraints.Values.Any(t => t.Item1 <= 0))
                throw new ArgumentException("Target values need to be greater zero", "pseudoAttributeConstraints");
        }

        /// <summary>
        /// Creates new AdvancesSolverSettings.
        /// </summary>
        /// <param name="baseSettings">Base settings to copy.</param>
        /// <param name="initialAttributes">Starting attributes of stats that calculations are based on.</param>
        /// <param name="attributeConstraints">The attribute constraints the solver should try to fullfill.</param>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints the solver should try to fullfill.</param>
        /// <param name="weaponClass">WeaponClass used for pseudo attribute calculation.</param>
        /// <param name="tags">Tags used for pseudo attribute calculation.</param>
        /// <param name="offHand">OffHand used for pseudo attribute calculation.</param>
        public AdvancedSolverSettings(SolverSettings baseSettings,
            Dictionary<string, float> initialAttributes,
            Dictionary<string, Tuple<float, double>> attributeConstraints,
            Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints,
            WeaponClass weaponClass, Tags tags, OffHand offHand)
            : this(baseSettings.Level, baseSettings.TotalPoints, baseSettings.Checked, baseSettings.Crossed,
                baseSettings.SubsetTree, baseSettings.InitialTree, initialAttributes,
                attributeConstraints, pseudoAttributeConstraints, weaponClass, tags, offHand)
        { }
    }
}