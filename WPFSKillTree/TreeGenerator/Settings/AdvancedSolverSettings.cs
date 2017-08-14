using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;

namespace POESKillTree.TreeGenerator.Settings
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    using SmallDigit =
#if (PoESkillTree_UseSmallDec_ForAttributes)
    SmallDec;
#else
    System.Single;
#endif
    using WeightType =
#if (PoESkillTree_AlternativeWeightType)
    SmallDec;
#else
    System.Double;
#endif
    /// <summary>
    /// Data class for settings for AdvancedSolver.
    /// </summary>
    public class AdvancedSolverSettings : SolverSettings
    {
        /// <summary>
        /// Maximum for points spent in the result tree.
        /// May be ignored if the only goal of the solver is to minimize point count.
        /// </summary>
        public readonly int TotalPoints;

        /// <summary>
        /// The attribute constraints the solver should try to fulfill.
        /// The key is the name of the attribute.
        /// Value is a tuple of target value (SmallDigit) and weight (double).
        /// Weight must be between 0 and 1 (both inclusive).
        /// </summary>
        public readonly Dictionary<string, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
        , SmallDigit
#endif
        >> AttributeConstraints;

        /// <summary>
        /// The pseudo attribute constraints the solver should try to fulfill.
        /// The key is the name of the attribute.
        /// Value is a tuple of target value (SmallDigit) and weight (double).
        /// Weight must be between 0 and 1 (both inclusive).
        /// </summary>
        public readonly Dictionary<PseudoAttribute, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
        , SmallDigit
#endif
        >> PseudoAttributeConstraints;

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
        public readonly Dictionary<string, SmallDigit> InitialAttributes;

        /// <summary>
        /// Creates new AdvancesSolverSettings.
        /// </summary>
        /// <param name="baseSettings">Base settings to copy.</param>
        /// <param name="totalPoints">Maximum for points spent in the result tree. (>= 0)</param>
        /// <param name="initialAttributes">Starting attributes of stats that calculations are based on.</param>
        /// <param name="attributeConstraints">The attribute constraints the solver should try to fullfill.</param>
        /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints the solver should try to fullfill.</param>
        /// <param name="weaponClass">WeaponClass used for pseudo attribute calculation.</param>
        /// <param name="tags">Tags used for pseudo attribute calculation.</param>
        /// <param name="offHand">OffHand used for pseudo attribute calculation.</param>
        public AdvancedSolverSettings(SolverSettings baseSettings, int totalPoints,
            Dictionary<string, SmallDigit> initialAttributes,
            Dictionary<string, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
            , SmallDigit
#endif
            >> attributeConstraints,
            Dictionary<PseudoAttribute, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
            , SmallDigit
#endif
            >> pseudoAttributeConstraints,
            WeaponClass weaponClass, Tags tags, OffHand offHand)
            : base(baseSettings)
        {
            if (totalPoints < 0) throw new ArgumentOutOfRangeException(nameof(totalPoints), totalPoints, "must be >= 0");

            TotalPoints = totalPoints;
            WeaponClass = weaponClass;
            Tags = tags;
            OffHand = offHand;
            AttributeConstraints = attributeConstraints ?? new Dictionary<string, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
        , SmallDigit
#endif
            >>();
            PseudoAttributeConstraints = pseudoAttributeConstraints ?? new Dictionary<PseudoAttribute, Tuple<SmallDigit, WeightType
#if (PoESkillTree_EnableMinimumValue)
        , SmallDigit
#endif
            >>();
            InitialAttributes = initialAttributes ?? new Dictionary<string, SmallDigit>();

            if (AttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
                throw new ArgumentException("Weights need to be between 0 and 1", "attributeConstraints");
            if (AttributeConstraints.Values.Any(t => t.Item1 <= 0))
                throw new ArgumentException("Target values need to be greater than zero", "attributeConstraints");
            if (PseudoAttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
                throw new ArgumentException("Weights need to be between 0 and 1", "pseudoAttributeConstraints");
            if (PseudoAttributeConstraints.Values.Any(t => t.Item1 <= 0))
                throw new ArgumentException("Target values need to be greater than zero", "pseudoAttributeConstraints");
#if (PoESkillTree_EnableMinimumValue)
            if (AttributeConstraints.Values.Any(t => t.Item3 < 0))
                throw new ArgumentException("Minimum values can not be negative", "attributeConstraints");
            if (PseudoAttributeConstraints.Values.Any(t => t.Item3 < 0))
                throw new ArgumentException("Minimum values can not be negative", "pseudoAttributeConstraints");
#endif
        }
    }
}