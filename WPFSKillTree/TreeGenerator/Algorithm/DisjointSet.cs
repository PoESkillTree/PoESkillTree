namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Describes a disjoint set data structure of incremental ints.
    /// Union and find operations have an average O(\alpha(n)) runtime
    /// (inverse Ackermann function, practically constant).
    /// </summary>
    /// <remarks>
    /// Algorithm based on https://en.wikipedia.org/wiki/Disjoint-set_data_structure
    /// </remarks>
    public class DisjointSet
    {
        private readonly int[] _parent;

        private readonly int[] _rank;

        /// <summary>
        /// Creates a new set containing the given number of nodes.
        /// </summary>
        public DisjointSet(int count)
        {
            _parent = new int[count];
            _rank = new int[count];
            // Initialize parents with themselves. Ranks are all initally 0.
            for (var i = 0; i < count; i++)
            {
                _parent[i] = i;
            }
        }

        /// <summary>
        /// Unions the sets of the given nodes.
        /// </summary>
        public void Union(int x, int y)
        {
            var xRoot = Find(x);
            var yRoot = Find(y);
            if (xRoot == yRoot) return;

            var xRank = _rank[xRoot];
            var yRank = _rank[yRoot];
            if (xRank < yRank)
            {
                _parent[xRoot] = yRoot;
            }
            else if (xRank > yRank)
            {
                _parent[yRoot] = xRoot;
            }
            else
            {
                _parent[yRoot] = xRoot;
                _rank[xRoot]++;
            }
        }

        /// <summary>
        /// Returns the representative node of the set the given node belongs to.
        /// </summary>
        public int Find(int x)
        {
            if (_parent[x] != x)
            {
                _parent[x] = Find(_parent[x]);
            }
            return _parent[x];
        }
    }
}