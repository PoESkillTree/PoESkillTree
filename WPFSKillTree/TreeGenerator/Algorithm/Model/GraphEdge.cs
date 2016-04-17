using System;
using System.Diagnostics;

namespace POESKillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    /// Represents a unidirectional edge between two nodes represented by
    /// their DistancesIndex.
    /// Two GraphEdges are equal, if they span the same nodes.
    /// </summary>
    [DebuggerDisplay("{N1}-{N2}:{Weight}")]
    public class GraphEdge : IEquatable<GraphEdge>
    {
        /// <summary>
        /// The two nodes of the edge. The first is always the smaller one.
        /// </summary>
        public readonly int N1, N2;
        
        public readonly uint Weight;

        /// <summary>
        /// Creates a new edge between the two nodes with the given weight.
        /// The node with the smaller index is stored as <see cref="N1"/>.
        /// </summary>
        public GraphEdge(int n1, int n2, uint weight)
        {
            N1 = Math.Min(n1, n2);
            N2 = Math.Max(n1, n2);
            Weight = weight;
        }

        public bool Equals(GraphEdge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return N1 == other.N1 && N2 == other.N2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GraphEdge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (N1 * 397) ^ N2;
            }
        }
    }
}