using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a path in <see cref="IStat"/> calculation subgraphs. A node for each <see cref="NodeType"/> can exist
    /// on each path. Except the <see cref="NodeType"/>s with "Total" in their name, these only exist on the main path.
    /// <para>
    /// A path consists of its <see cref="IModifierSource"/> and the <see cref="IStat"/>s its
    /// <see cref="NodeType.Base"/> is converted from (which are none if no conversion is applied).
    /// The "main path" refers to the global <see cref="IModifierSource"/> without conversions.
    /// </para>
    /// </summary>
    public class PathDefinition
    {
        /// <summary>
        /// An instance of the main path (global <see cref="IModifierSource"/>, no conversions).
        /// </summary>
        public static readonly PathDefinition MainPath = new PathDefinition(new GlobalModifierSource());

        public PathDefinition(IModifierSource modifierSource, params IStat[] conversiStats)
            : this(modifierSource, (IReadOnlyList<IStat>) conversiStats)
        {
        }

        public PathDefinition(IModifierSource modifierSource, IReadOnlyList<IStat> conversionStats)
        {
            ModifierSource = modifierSource;
            ConversionStats = conversionStats;
        }

        /// <summary>
        /// The canonical modifier source of this path.
        /// </summary>
        public IModifierSource ModifierSource { get; }

        /// <summary>
        /// The stats on the conversion path, not including the stat subgraph this path belongs to.
        /// Empty if unconverted.
        /// </summary>
        public IReadOnlyList<IStat> ConversionStats { get; }

        /// <summary>
        /// True if this instance describes the main path.
        /// </summary>
        public bool IsMainPath => Equals(MainPath);

        public override bool Equals(object obj) => 
            (obj == this) || (obj is PathDefinition other && Equals(other));

        private bool Equals(PathDefinition other) =>
            ModifierSource.Equals(other.ModifierSource) && ConversionStats.SequenceEqual(other.ConversionStats);

        public override int GetHashCode() => 
            (ModifierSource, ConversionStats.SequenceHash()).GetHashCode();
    }
}