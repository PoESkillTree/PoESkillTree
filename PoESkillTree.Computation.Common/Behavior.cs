using System;
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
        public IReadOnlyList<IStat> AffectedStats { get; }

        /// <summary>
        /// The <see cref="NodeType"/>s of nodes affected by this behavior.
        /// </summary>
        public IReadOnlyList<NodeType> AffectedNodeTypes { get; }

        /// <summary>
        /// The <see cref="PathDefinition"/>s of nodes affected by this behavior.
        /// </summary>
        public BehaviorPathInteraction AffectedPaths { get; }

        /// <summary>
        /// The transformation applied by this behavior.
        /// </summary>
        public IValueTransformation Transformation { get; }

        protected override object ToTuple()
            => (WithSequenceEquality(AffectedStats), WithSequenceEquality(AffectedNodeTypes), AffectedPaths,
                Transformation);
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