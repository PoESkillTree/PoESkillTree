using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a behavior. A behavior applies an <see cref="IValueTransformation"/> to a defined set of calculation
    /// graph nodes, thus modifying the values calculated by those nodes.
    /// </summary>
    public class Behavior
    {
        /*
         * "Modifiers to Foo also apply to Bar [at x% of their value]":
         * - Applies to NodeType.Base and PathTotal of Bar.
         * - When those nodes request the value of BaseAdd, Increase or More the respective value of
         *   Foo is multiplied by x/100 and added to the original value.
         *   (some do not apply to Base/BaseAdd)
         * - Each Foo, Bar combination will result in one stat with the respective behavior. The value of that stat is
         *   used as the multiplier.
         *   - These stats are explicitly registered so that UI can add the newly affecting modifiers to its tables
         *   - This explicit registration is handled differently by the UI (they are not stats that need to be set by
         *     users). More specialized registration may be necessary.
         * - Also works for "Modifiers to Spell Damage apply to this Skill's Damage Over Time effect".
         *   "this Skill's" is part of the condition (modifier source Local->Skill), everything else is the same
         * - Affects all paths (if behaviors applying to Base of stats partaking in conversions exist, I don't know
         *   whether they should apply to conversion paths)
         * "Modifiers to Claw Foo also apply to Unarmed":
         * - Affecting form nodes open a whole bunch of new issues, e.g. the same form node can be used in different
         *   stats, so this can't be done as a behavior. Solutions need to be solely in the builder implementations.
         * Effectiveness of Added Damage:
         * - Applies to NodeType.BaseAdd of all damage stats
         * - Values of requested form nodes are multiplied by the effectiveness stat's value
         * - Affects all paths (only non-conversion paths have BaseAdd nodes)
         * Rounding:
         * - Each stat can have different rounding behaviors
         * - This can affect nodes of all NodeTypes
         * - Modifies the output of affected nodes by rounding it.
         * - Affects all paths
         * Default values:
         * - Affects NodeType.BaseSet of the stat
         * - Modifies the output by changing null to the default value if the BaseSet nodes of non-main, non-converted
         *   paths are also null
         * - With this, NodeValueAggregators.CalculateBaseSet() should default to null
         * - Only affects the main path
         */

        public Behavior(IEnumerable<IStat> affectedStats, IEnumerable<NodeType> affectedNodeTypes,
            BehaviorPathInteraction affectedPaths, IValueTransformation transformation)
        {
            AffectedStats = affectedStats;
            AffectedNodeTypes = affectedNodeTypes;
            AffectedPaths = affectedPaths;
            Transformation = transformation;
        }

        /// <summary>
        /// The <see cref="IStat"/> subgraphs affected by this behavior.
        /// </summary>
        public IEnumerable<IStat> AffectedStats { get; }

        /// <summary>
        /// The <see cref="NodeType"/>s of nodes affected by this behavior.
        /// </summary>
        public IEnumerable<NodeType> AffectedNodeTypes { get; }

        /// <summary>
        /// The <see cref="PathDefinition"/>s of nodes affected by this behavior.
        /// </summary>
        public BehaviorPathInteraction AffectedPaths { get; }

        /// <summary>
        /// The transformation applied by this behavior.
        /// </summary>
        public IValueTransformation Transformation { get; }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is Behavior other && Equals(other));

        private bool Equals(Behavior other) =>
            Transformation.Equals(other.Transformation) && AffectedPaths.Equals(other.AffectedPaths) &&
            AffectedStats.SequenceEqual(other.AffectedStats) &&
            AffectedNodeTypes.SequenceEqual(other.AffectedNodeTypes);

        public override int GetHashCode() =>
            (AffectedStats.SequenceHash(), AffectedNodeTypes.SequenceHash(), AffectedPaths, Transformation)
            .GetHashCode();
    }


    /// <summary>
    /// Defines the <see cref="PathDefinition"/>s affected by a behavior.
    /// </summary>
    public enum BehaviorPathInteraction
    {
        /// <summary>
        /// The behavior affects all paths.
        /// </summary>
        AllPaths,

        /// <summary>
        /// The behavior only affects the main path (paths with <see cref="PathDefinition.IsMainPath"/>).
        /// </summary>
        MainPathOnly,

        /// <summary>
        /// The behavior only affects conversion paths (paths where <see cref="PathDefinition.ConversionStats"/> is not
        /// empty).
        /// </summary>
        ConversionPathsOnly
    }
}