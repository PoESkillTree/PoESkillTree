using System.Collections.Generic;
using MoreLinq;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a path in <see cref="IStat"/> calculation subgraphs. A node for each <see cref="NodeType"/> can exist
    /// on each path. Except the <see cref="NodeType"/>s with "Total" in their name, these only exist on the main path.
    /// <para>
    /// A path consists of its <see cref="ModifierSource"/> and the <see cref="IStat"/>s its
    /// <see cref="NodeType.Base"/> is converted from (which are none if no conversion is applied).
    /// The "main path" refers to the global <see cref="ModifierSource"/> without conversions.
    /// </para>
    /// </summary>
    public class PathDefinition
    {
        /// <summary>
        /// An instance of the main path (global <see cref="ModifierSource"/>, no conversions).
        /// </summary>
        public static readonly PathDefinition MainPath = new PathDefinition(new ModifierSource.Global());

        private readonly IStat[] _conversionStats;

        public PathDefinition(ModifierSource modifierSource, params IStat[] conversionStats)
        {
            ModifierSource = modifierSource;
            _conversionStats = conversionStats;
        }

        /// <summary>
        /// The canonical modifier source of this path.
        /// </summary>
        public ModifierSource ModifierSource { get; }

        /// <summary>
        /// The stats on the conversion path, not including the stat subgraph this path belongs to.
        /// Empty if unconverted.
        /// </summary>
        public IReadOnlyList<IStat> ConversionStats => _conversionStats;

        /// <summary>
        /// True if this instance describes the main path.
        /// </summary>
        public bool IsMainPath => Equals(MainPath);

        public override bool Equals(object obj)
            => (obj == this) || (obj is PathDefinition other && Equals(other));

        private bool Equals(PathDefinition other)
            => GetType() == other.GetType() &&
               ModifierSource == other.ModifierSource &&
               EqualConversionStats(other);

        private bool EqualConversionStats(PathDefinition other)
        {
            if (_conversionStats.Length != other._conversionStats.Length)
                return false;

            for (var i = 0; i < _conversionStats.Length; i++)
            {
                if (!_conversionStats[i].Equals(other._conversionStats[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                hash = hash * 31 + ModifierSource.GetHashCode();
                foreach (var value in _conversionStats)
                {
                    hash = hash * 31 + value.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
            => $"({ModifierSource}, {ConversionStats.ToDelimitedString(", ")})";
    }
}