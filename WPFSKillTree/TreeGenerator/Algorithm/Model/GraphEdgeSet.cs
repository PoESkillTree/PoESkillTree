using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    /// A set of <see cref="GraphEdge"/>s.
    /// </summary>
    public class GraphEdgeSet : IReadOnlyCollection<GraphEdge>
    {
        // Dictionary to enable constant time access of the stored edges. Not possible with HashSets.
        private readonly Dictionary<GraphEdge, GraphEdge> _edgeDict = new Dictionary<GraphEdge, GraphEdge>();
        
        private readonly HashSet<int>[] _adjacencyMatrix;

        public int Count => _edgeDict.Count;

        public GraphEdge this[int n1, int n2] => _edgeDict[CreateTmpEdge(n1, n2)];

        /// <summary>
        /// Creates a new GraphEdgeSet with nodes from 0 to <paramref name="nodeCount"/> (exclusive).
        /// All nodes initially have no neighbors.
        /// </summary>
        public GraphEdgeSet(int nodeCount)
        {
            _adjacencyMatrix = Enumerable.Range(0, nodeCount).Select(_ => new HashSet<int>()).ToArray();
        }

        public IReadOnlyList<int> NeighborsOf(int node)
        {
            return _adjacencyMatrix[node].ToList();
        }

        /// <summary>
        /// Returns all edges of the given node.
        /// </summary>
        public IReadOnlyList<GraphEdge> EdgesOf(int node)
        {
            return _adjacencyMatrix[node].Select(n2 => this[node, n2]).ToList();
        }

        /// <summary>
        /// Creates a new <see cref="GraphEdge"/> with the given parameters and adds it to the set.
        /// </summary>
        public void Add(int n1, int n2, uint weight)
        {
            var edge = new GraphEdge(n1, n2, weight);
            // Assigning with indexer does not override the existing value ...
            _edgeDict.Remove(edge);
            _edgeDict[edge] = edge;
            _adjacencyMatrix[edge.N1].Add(edge.N2);
            _adjacencyMatrix[edge.N2].Add(edge.N1);
        }

        /// <summary>
        /// Removes the given edge from the set.
        /// </summary>
        public void Remove(GraphEdge edge)
        {
            _adjacencyMatrix[edge.N1].Remove(edge.N2);
            _adjacencyMatrix[edge.N2].Remove(edge.N1);
            _edgeDict.Remove(edge);
        }

        /// <summary>
        /// Removes the edge between the given nodes from the set.
        /// </summary>
        public void Remove(int n1, int n2)
        {
            Remove(CreateTmpEdge(n1, n2));
        }

        /// <summary>
        /// Creates a temporary edge to access the stored edge.
        /// </summary>
        private static GraphEdge CreateTmpEdge(int n1, int n2)
        {
            return new GraphEdge(n1, n2, 0);
        }

        public IEnumerator<GraphEdge> GetEnumerator()
        {
            return _edgeDict.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}