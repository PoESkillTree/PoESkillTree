using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public interface IReadOnlyGraphEdgeSet : IReadOnlyCollection<GraphEdge>
    {
        bool HasNeighbors(int node);

        IReadOnlyList<int> NeighborsOf(int node);
    }

    class GraphEdgeSet : ICollection<GraphEdge>, IReadOnlyGraphEdgeSet
    {
        private readonly Dictionary<GraphEdge, GraphEdge> _edgeDict = new Dictionary<GraphEdge, GraphEdge>();

        private readonly HashSet<int>[] _adjacencyMatrix;

        public int Count { get { return _edgeDict.Count; } }

        public bool IsReadOnly { get { return false; } }

        public GraphEdge this[int n1, int n2]
        {
            get { return _edgeDict[CreateTmpEdge(n1, n2)]; }
        }

        public GraphEdgeSet(int nodeCount)
        {
            _adjacencyMatrix = Enumerable.Range(0, nodeCount).Select(_ => new HashSet<int>()).ToArray();
        }

        public IReadOnlyList<int> NeighborsOf(int node)
        {
            return _adjacencyMatrix[node].ToList();
        }

        public bool HasNeighbors(int node)
        {
            return _adjacencyMatrix[node].Any();
        }

        public void Add(GraphEdge edge)
        {
            _edgeDict[edge] = edge;
            _adjacencyMatrix[edge.N1].Add(edge.N2);
            _adjacencyMatrix[edge.N2].Add(edge.N1);
        }

        public void Clear()
        {
            foreach (var set in _adjacencyMatrix)
            {
                set.Clear();
            }
            _edgeDict.Clear();
        }

        public bool Contains(GraphEdge item)
        {
            return _edgeDict.ContainsKey(item);
        }

        public void CopyTo(GraphEdge[] array, int arrayIndex)
        {
            _edgeDict.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(GraphEdge edge)
        {
            _adjacencyMatrix[edge.N1].Remove(edge.N2);
            _adjacencyMatrix[edge.N2].Remove(edge.N1);
            return _edgeDict.Remove(edge);
        }

        public void Remove(int n1, int n2)
        {
            Remove(CreateTmpEdge(n1, n2));
        }

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