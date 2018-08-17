using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a behavior. A behavior applies an <see cref="IValueTransformation"/> to a defined set of calculation
    /// graph nodes, thus modifying the values calculated by those nodes.
    /// </summary>
    public class Behavior
    {
        /*
         * Effectiveness of Added Damage:
         * - Applies to NodeType.BaseAdd of all damage stats
         * - Values of requested form nodes are multiplied by the effectiveness stat's value
         * - Affects all paths (only non-conversion paths have BaseAdd nodes)
         * Rounding:
         * - Each stat can have different rounding behaviors
         * - This can affect nodes of all NodeTypes
         * - Modifies the output of affected nodes by rounding it.
         * - Affects all paths
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
    [Flags]
    public enum BehaviorPathInteraction
    {
        /// <summary>
        /// The behavior affects the main path (paths with <see cref="PathDefinition.IsMainPath"/>).
        /// </summary>
        Main = 1,

        /// <summary>
        /// The behavior affects conversion paths (paths where <see cref="PathDefinition.ConversionStats"/> is not
        /// empty).
        /// </summary>
        Conversion = 2,

        /// <summary>
        /// The behavior affects non-conversion paths (paths where <see cref="PathDefinition.ConversionStats"/> is
        /// empty).
        /// </summary>
        NonConversion = 4,

        /// <summary>
        /// The behavior affects all paths.
        /// </summary>
        All = Main | Conversion | NonConversion,
    }
}