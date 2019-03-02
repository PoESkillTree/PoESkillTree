using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a behavior. A behavior applies an <see cref="IValueTransformation"/> to a defined set of calculation
    /// graph nodes, thus modifying the values calculated by those nodes.
    /// </summary>
    public class Behavior : ValueObject
    {
        /*
         * Rounding:
         * - Each stat can have different rounding behaviors
         * - This can affect nodes of all NodeTypes
         * - Modifies the output of affected nodes by rounding it.
         * - Affects all paths
         */

        public Behavior(IReadOnlyList<IStat> affectedStats, IReadOnlyList<NodeType> affectedNodeTypes,
            IBehaviorPathRule affectedPathsRule, IValueTransformation transformation)
            : base(true)
        {
            AffectedStats = affectedStats;
            AffectedNodeTypes = affectedNodeTypes;
            AffectedPathsRule = affectedPathsRule;
            Transformation = transformation;
        }

        /// <summary>
        /// The <see cref="IStat"/> subgraphs affected by this behavior.
        /// </summary>
        public IReadOnlyList<IStat> AffectedStats { get; }

        /// <summary>
        /// The <see cref="NodeType"/>s of nodes affected by this behavior.
        /// </summary>
        public IReadOnlyList<NodeType> AffectedNodeTypes { get; }
        
        public IBehaviorPathRule AffectedPathsRule { get; }

        /// <summary>
        /// The transformation applied by this behavior.
        /// </summary>
        public IValueTransformation Transformation { get; }

        protected override object ToTuple()
            => (WithSequenceEquality(AffectedStats), WithSequenceEquality(AffectedNodeTypes), AffectedPathsRule,
                Transformation);
    }
}