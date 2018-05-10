using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class PathDefinition
    {
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

        // The canonical modifier source of this path (Global is the main path)
        public IModifierSource ModifierSource { get; }

        // The stats on the conversion path, not including the stat subgraph this path belongs to (empty if unconverted)
        public IReadOnlyList<IStat> ConversionStats { get; }

        public override bool Equals(object obj) => 
            (obj == this) || (obj is PathDefinition other && Equals(other));

        private bool Equals(PathDefinition other) =>
            ModifierSource.Equals(other.ModifierSource) && ConversionStats.SequenceEqual(other.ConversionStats);

        public override int GetHashCode() => 
            (ModifierSource, ConversionStats).GetHashCode();
    }
}