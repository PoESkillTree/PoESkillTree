using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PoESkillTree.TreeGenerator.Algorithm.Model;

[assembly: InternalsVisibleTo("UnitTests")]
namespace PoESkillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Interface that serves as a cache for uint distances between nodes represented as
    /// ints between 0 and <see cref="CacheSize"/>.
    /// </summary>
    public interface IDistanceLookup
    {
        /// <summary>
        /// Gets the number of cached nodes. CacheSize - 1 is the maximum index that can be used
        /// for <see cref="this"/>.
        /// </summary>
        int CacheSize { get; }

        /// <summary>
        /// Gets the stored distance between a and b.
        /// </summary>
        uint this[int a, int b] { get; }
    }

    /// <summary>
    /// Interface that provides shortest path between nodes in addition to what
    /// <see cref="IDistanceLookup"/> provides.
    /// </summary>
    public interface IDistancePathLookup : IDistanceLookup
    {
        IReadOnlyList<ushort> GetShortestPath(int a, int b);
    }

    /// <summary>
    /// Exception that is thrown if an operation can't be continued because the
    /// graph is disconnected.
    /// </summary>
    public class GraphNotConnectedException : Exception
    {
    }

    /// <summary>
    ///  Calculates and caches distances between nodes. Only relies on adjacency
    ///  information stored in the nodes.
    /// </summary>
    public class DistanceLookup : IDistancePathLookup
    {
        
        private uint[,] _distances;

        private ushort[,][] _paths;

        /// <summary>
        /// The GraphNodes of which distances and paths are cached.
        /// The index in the Array equals their <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        private GraphNode[] _nodes;

        public int CacheSize { get; private set; }

        /// <summary>
        /// Retrieves the path distance from one node to another.
        /// CalculateFully must have been called or an exception will be thrown.
        /// </summary>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        /// <remarks>
        ///  If the nodes are not connected, 0 will be returned.
        ///  If at least one of the nodes is greater or equals CacheSize, a IndexOutOfRangeException will be thrown.
        /// </remarks>
        public uint this[int a, int b]
        {
            get { return _distances[a, b]; }
            private set { _distances[a, b] = _distances[b, a] = value; }
        }

        /// <summary>
        ///  Retrieves the shortest path from one node to another.
        /// </summary>
        /// <param name="a">The first graph node. (not null)</param>
        /// <param name="b">The second graph node. (not null)</param>
        /// <returns>The shortest path from a to b, not containing either and ordered from a to b or b to a.</returns>
        /// <remarks>
        ///  If the nodes are not connected, null will be returned.
        ///  If at least one of the nodes is greater or equals CacheSize, a IndexOutOfRangeException will be thrown.
        /// </remarks>
        public IReadOnlyList<ushort> GetShortestPath(int a, int b)
        {
            return _paths[a, b];
        }

        /// <summary>
        /// Sets the shortest path between the given two nodes.
        /// </summary>
        private void SetShortestPath(int a, int b, ushort[] path)
        {
            _paths[a, b] = _paths[b, a] = path;
        }

        /// <summary>
        /// Returns the GraphNode with the specified <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        public GraphNode IndexToNode(int index)
        {
            return _nodes[index];
        }

        /// <summary>
        /// Returns true iff the given nodes are connected.
        /// </summary>
        public bool AreConnected(GraphNode a, GraphNode b)
        {
            return GetShortestPath(a.DistancesIndex, b.DistancesIndex) != null;
        }

        /// <summary>
        /// Returns true iff the given nodes are connected.
        /// </summary>
        public bool AreConnected(int a, int b)
        {
            return GetShortestPath(a, b) != null;
        }

        /// <summary>
        /// Merges both nodes so that distances and paths to any of the two nodes are overwritten
        /// to the shortest distance and path to any of the two nodes or the nodes on the shortest path
        /// between them.
        /// Only the paths and distances from and to <paramref name="into"/> are updated.
        /// </summary>
        public void MergeInto(int x, int into)
        {
            var path = new HashSet<ushort>(GetShortestPath(x, into));
            this[x, into] = 0;
            SetShortestPath(x, into, new ushort[0]);
            for (var i = 0; i < CacheSize; i++)
            {
                if (i == into || i == x) continue;

                var ixPath = GetShortestPath(i, x).Where(n => !path.Contains(n)).ToArray();
                var iIntoPath = GetShortestPath(i, into).Where(n => !path.Contains(n)).ToArray();
                if (ixPath.Length < iIntoPath.Length)
                {
                    this[i, into] = (uint) ixPath.Length + 1;
                    SetShortestPath(i, into, ixPath);
                }
                else
                {
                    this[i, into] = (uint)iIntoPath.Length + 1;
                    SetShortestPath(i, into, iIntoPath);
                }
            }
        }

        /// <summary>
        /// Calculates and caches all distances between the given nodes.
        /// Sets DistancesIndex of the nodes as incremental index in the cache starting from 0.
        /// </summary>
        public DistanceLookup(IReadOnlyList<GraphNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");

            CacheSize = nodes.Count;
            _nodes = new GraphNode[CacheSize];
            for (var i = 0; i < CacheSize; i++)
            {
                nodes[i].DistancesIndex = i;
                _nodes[i] = nodes[i];
            }
            _distances = new uint[CacheSize, CacheSize];
            _paths = new ushort[CacheSize, CacheSize][];

            foreach (var node in nodes)
            {
                Dijkstra(node);
            }
        }

        /// <summary>
        /// Removes the given nodes from the cache.
        /// Resets DistancesIndex of removedNodes to -1 and of remainingNodes to be
        /// incremental without holes again.
        /// O(|removedNodes| + |remainingNodes|^2)
        /// </summary>
        /// <returns>List of the remaining node. Ordered by their distance index.</returns>
        public List<GraphNode> RemoveNodes(IEnumerable<GraphNode> removedNodes)
        {
            if (removedNodes == null) throw new ArgumentNullException("removedNodes");

            var removed = new bool[CacheSize];
            foreach (var node in removedNodes)
            {
                removed[node.DistancesIndex] = true;
                node.DistancesIndex = -1;
            }
            var remainingNodes = new List<GraphNode>();
            for (var i = 0; i < CacheSize; i++)
            {
                if (!removed[i])
                    remainingNodes.Add(IndexToNode(i));
            }

            var oldDistances = _distances;
            var oldPaths = _paths;
            CacheSize = remainingNodes.Count;
            _distances = new uint[CacheSize, CacheSize];
            _paths = new ushort[CacheSize, CacheSize][];

            for (var i = 0; i < CacheSize; i++)
            {
                var oldi = remainingNodes[i].DistancesIndex;
                for (var j = 0; j < CacheSize; j++)
                {
                    var oldj = remainingNodes[j].DistancesIndex;
                    _distances[i, j] = oldDistances[oldi, oldj];
                    _paths[i, j] = oldPaths[oldi, oldj];
                }
            }

            _nodes = new GraphNode[CacheSize];
            for (var i = 0; i < CacheSize; i++)
            {
                remainingNodes[i].DistancesIndex = i;
                _nodes[i] = remainingNodes[i];
            }

            return remainingNodes;
        }

        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node and calculate distances and shortest paths to all reachable relevant nodes.
        /// </summary>
        /// <param name="start">The starting node. (not null)</param>
        private void Dijkstra(GraphNode start)
        {
            if (start == null) throw new ArgumentNullException("start");

            AddEdge(start, start, -1, null);

            // The last newly found nodes.
            var front = new HashSet<GraphNode>() { start };
            // The already visited nodes.
            var visited = new HashSet<GraphNode>() { start };
            // The dictionary of the predecessors of the visited nodes.
            var predecessors = new Dictionary<ushort, ushort>();
            // The traversed distance from the starting node in edges.
            var distFromStart = 0;

            while (front.Count > 0)
            {
                var newFront = new HashSet<GraphNode>();

                foreach (var node in front)
                {
                    foreach (var adjacentNode in node.Adjacent)
                    {
                        if (visited.Contains(adjacentNode))
                            continue;

                        predecessors[adjacentNode.Id] = node.Id;
                        
                        if (adjacentNode.DistancesIndex >= 0)
                        {
                            AddEdge(start, adjacentNode, distFromStart, predecessors);
                        }

                        newFront.Add(adjacentNode);
                        visited.Add(adjacentNode);
                    }
                }

                front = newFront;
                distFromStart++;
            }
        }

        /// <summary>
        /// Adds the distance and shortest path between from and to to the respectives
        /// dictionarys if not already present.
        /// </summary>
        private void AddEdge(GraphNode from, GraphNode to, int distFromStart, IDictionary<ushort, ushort> predecessors)
        {
            var length = distFromStart + 1;
            
            var i1 = from.DistancesIndex;
            var i2 = to.DistancesIndex;
            if (_paths[i1, i2] != null) return;

            var path = length > 0 ? GenerateShortestPath(from.Id, to.Id, predecessors, length) : new ushort[0];
            this[i1, i2] = (uint) length;
            SetShortestPath(i1, i2, path);
        }
        
        /// <summary>
        /// Generates the shortest path from target to start by reading it out of the predecessors-dictionary.
        /// The dictionary must have a path from target to start stored.
        /// </summary>
        /// <param name="start">The starting node</param>
        /// <param name="target">The target node</param>
        /// <param name="predecessors">Dictonary with the predecessor of every node</param>
        /// <param name="length">Length of the shortest path</param>
        /// <returns>The shortest path from start to target, not including either. The Array is ordered from target to start</returns>
        private static ushort[] GenerateShortestPath(ushort start, ushort target, IDictionary<ushort, ushort> predecessors, int length)
        {
            var path = new ushort[length - 1];
            var i = 0;
            for (var node = predecessors[target]; node != start; node = predecessors[node], i++)
            {
                path[i] = node;
            }
            return path;
        }
    }
}
