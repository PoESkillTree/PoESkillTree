using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]
namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    ///  Calculates and caches distances between nodes. Only relies on adjacency
    ///  information stored in the nodes.
    /// </summary>
    public class DistanceLookup
    {
        // The uint compounds both ushort indices.
        private Dictionary<uint, int> _distances = new Dictionary<uint, int>();

        private Dictionary<uint, ushort[]> _paths = new Dictionary<uint, ushort[]>();
        
        private int[,] _distancesFast;

        private ushort[,][] _pathsFast;

        /// <summary>
        /// The GraphNodes of which distances and paths are cached.
        /// The index in the Array equals their <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        private GraphNode[] _nodes;

        /// <summary>
        /// Whether CalculateFully got called.
        /// </summary>
        private bool _fullyCached;

        /// <summary>
        /// Number of cached nodes.
        /// </summary>
        private int _cacheSize = int.MaxValue;

        /// <summary>
        /// Gets the number of cached nodes.
        /// </summary>
        public int CacheSize
        {
            get { return _cacheSize; }
        }

        /// <summary>
        ///  Retrieves the path distance from one node to another, or calculates
        ///  it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="a">The first graph node.</param>
        /// <param name="b">The second graph node.</param>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        /// <remarks>
        ///  If CalculateFully has been called and the nodes are not connected, 0 will be returned.
        ///  If CalculateFully has not been called and the nodes are not connected, a GraphNotConnectedException will be thrown.
        /// </remarks>
        public int this[GraphNode a, GraphNode b]
        {
            get
            {
                if (_fullyCached)
                {
                    return _distancesFast[a.DistancesIndex, b.DistancesIndex];
                }

                var index = GetIndex(a, b);
                if (!_distances.ContainsKey(index))
                {
                    Dijkstra(a, b);
                }
                return _distances[index];
            }
        }

        /// <summary>
        /// Retrieves the path distance from one node to another.
        /// CalculateFully must have been called or an exception will be thrown.
        /// </summary>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        public int this[int a, int b]
        {
            get { return _distancesFast[a, b]; }
        }

        /// <summary>
        ///  Retrieves the shortest path from one node to another, or calculates
        ///  it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="a">The first graph node. (not null)</param>
        /// <param name="b">The second graph node. (not null)</param>
        /// <returns>The shortest path from a to b, not containing either and ordered from a to b or b to a.</returns>
        /// <remarks>
        ///  If CalculateFully has been called and the nodes are not connected, null will be returned.
        ///  If CalculateFully has not been called and the nodes are not connected, a GraphNotConnectedException will be thrown.
        /// </remarks>
        public ushort[] GetShortestPath(GraphNode a, GraphNode b)
        {
            if (_fullyCached)
            {
                return _pathsFast[a.DistancesIndex, b.DistancesIndex];
            }

            var index = GetIndex(a, b);
            if (!_distances.ContainsKey(index))
            {
                Dijkstra(a, b);
            }
            return _paths[index];
        }

        /// <summary>
        /// Returns the GraphNode with the specified <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        public GraphNode IndexToNode(int index)
        {
            return _nodes[index];
        }

        /// <summary>
        /// Returns whether the given nodes are connected.
        /// </summary>
        public bool AreConnected(GraphNode a, GraphNode b)
        {
            try
            {
                // Null if not connected and _fullyCached
                // Exception if not connected and not _fullyCached
                return GetShortestPath(a, b) != null;
            }
            catch (GraphNotConnectedException)
            {
                return false;
            }
        }

        /// <summary>
        ///  Compounds two ushort node indices into a single uint one, which
        ///  is independent of the order of the two indices.
        /// </summary>
        /// <param name="a">The first index.</param>
        /// <param name="b">The second index.</param>
        /// <returns>The compounded index.</returns>
        private static uint GetIndex(GraphNode a, GraphNode b)
        {
            var aId = a.Id;
            var bId = b.Id;
            return (uint)(Math.Min(aId, bId) << 16) + Math.Max(aId, bId);
        }

        /// <summary>
        /// Calculates and caches all distances between the given nodes.
        /// Enables fast lookups.
        /// Sets DistancesIndex of the nodes as incremental index in the cache starting from 0.
        /// </summary>
        /// <remarks>Calls to GetDistance and GetShortestPath after this method
        /// has been called must already be cached or exceptions will be thrown.</remarks>
        public void CalculateFully(List<GraphNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException("nodes");

            _cacheSize = nodes.Count;
            _nodes = new GraphNode[_cacheSize];
            for (var i = 0; i < _cacheSize; i++)
            {
                nodes[i].DistancesIndex = i;
                _nodes[i] = nodes[i];
            }
            _distancesFast = new int[_cacheSize, _cacheSize];
            _pathsFast = new ushort[_cacheSize, _cacheSize][];

            _fullyCached = true;
            foreach (var node in nodes)
            {
                Dijkstra(node);
            }

            // No longer needed.
            _distances = null;
            _paths = null;
        }

        /// <summary>
        /// Removes the given nodes from the cache.
        /// Resets DistancesIndex of removedNodes to -1 and of remainingNodes to be
        /// incremental without holes again.
        /// </summary>
        public void RemoveNodes(List<GraphNode> removedNodes, List<GraphNode> remainingNodes)
        {
            if (removedNodes == null) throw new ArgumentNullException("removedNodes");
            if (remainingNodes == null) throw new ArgumentNullException("remainingNodes");

            foreach (var node in removedNodes)
            {
                node.DistancesIndex = -1;
            }

            var oldDistances = _distancesFast;
            var oldPaths = _pathsFast;
            _cacheSize = remainingNodes.Count;
            _distancesFast = new int[_cacheSize, _cacheSize];
            _pathsFast = new ushort[_cacheSize, _cacheSize][];

            for (var i = 0; i < _cacheSize; i++)
            {
                var oldi = remainingNodes[i].DistancesIndex;
                for (var j = 0; j < _cacheSize; j++)
                {
                    var oldj = remainingNodes[j].DistancesIndex;
                    _distancesFast[i, j] = oldDistances[oldi, oldj];
                    _pathsFast[i, j] = oldPaths[oldi, oldj];
                }
            }

            _nodes = new GraphNode[_cacheSize];
            for (var i = 0; i < _cacheSize; i++)
            {
                remainingNodes[i].DistancesIndex = i;
                _nodes[i] = remainingNodes[i];
            }
        }

        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node until the target node is found (if specified) or until all marked nodes got checked.
        /// </summary>
        /// <param name="start">The starting node. (not null)</param>
        /// <param name="target">The (optional) target node.</param>
        private void Dijkstra(GraphNode start, GraphNode target = null)
        {
            if (start == null) throw new ArgumentNullException("start");
            if (start == target) return;

            // The last newly found nodes.
            var front = new HashSet<GraphNode>() { start };
            // The already visited nodes.
            var visited = new HashSet<GraphNode>() { start };
            // The dictionary of the predecessors of the visited nodes.
            var predecessors = new Dictionary<ushort, ushort>();
            // The traversed distance from the starting node in edges.
            var distFromStart = 0;

            var nodesLeft = _cacheSize;
            if (start.DistancesIndex >= 0)
            {
                // Don't count the start node.
                nodesLeft--;
            }

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

                        if (adjacentNode == target)
                        {
                            AddEdge(start, adjacentNode, distFromStart, predecessors);
                            return;
                        }
                        if (adjacentNode.DistancesIndex >= 0)
                        {
                            AddEdge(start, adjacentNode, distFromStart, predecessors);
                            nodesLeft--;
                            if (nodesLeft == 0) return;
                        }

                        newFront.Add(adjacentNode);
                        visited.Add(adjacentNode);
                    }
                }

                front = newFront;
                distFromStart++;
            }

            if (target != null)
            {
                throw new GraphNotConnectedException();
            }
        }

        /// <summary>
        /// Adds the distance and shortest path between from and to to the respectives
        /// dictionarys if not already present.
        /// </summary>
        private void AddEdge(GraphNode from, GraphNode to, int distFromStart, IDictionary<ushort, ushort> predecessors)
        {
            var index = GetIndex(from, to);
            if (_distances.ContainsKey(index))
            {
                return;
            }

            var length = distFromStart + 1;
            var path = GenerateShortestPath(from.Id, to.Id, predecessors, length);

            if (_fullyCached)
            {
                var i1 = from.DistancesIndex;
                var i2 = to.DistancesIndex;
                _distancesFast[i1, i2] = _distancesFast[i2, i1] = length;
                _pathsFast[i1, i2] = _pathsFast[i2, i1] = path;
            }
            else
            {
                _paths[index] = path;
            }
            // Distances is saved either way to be able to skip already added distances.
            _distances[index] = length;
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
    
    internal class GraphNotConnectedException : Exception
    {
    }
}
