using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests")]
namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  Calculates and caches distances between nodes. Only relies on adjacency
    ///  information stored in the nodes.
    /// </summary>
    public class DistanceLookup
    {
        // The uint compounds both ushort indices.
        private readonly Dictionary<uint, int> _distances = new Dictionary<uint, int>();

        private readonly Dictionary<uint, ushort[]> _paths = new Dictionary<uint, ushort[]>();

        /// <summary>
        /// Whether CalculateFully got called.
        /// </summary>
        private bool _gotFullyCached;

        /// <summary>
        /// Number ob marked nodes given to CalculateFully.
        /// </summary>
        private int _markedNodeCount = int.MaxValue;

        /// <summary>
        ///  Retrieves the path distance from one node pf a graph edge to the other,
        ///  or calculates it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="edge">The graph edge.</param>
        /// <returns>The length of the graph edge (equals the amount of edges
        /// traversed).</returns>
        public int GetDistance(GraphEdge edge)
        {
            return GetDistance(edge.outside, edge.inside);
        }

        /// <summary>
        ///  Retrieves the path distance from one node to another, or calculates
        ///  it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="a">The first graph node.</param>
        /// <param name="b">The second graph node</param>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        public int GetDistance(GraphNode a, GraphNode b)
        {
            var index = GetIndex(a, b);

            if (_gotFullyCached)
            {
                return _distances[index];
            }

            if (!_distances.ContainsKey(index))
            {
                Dijkstra(a, b);
            }
            return _distances[index];
        }

        /// <summary>
        ///  Retrieves the shortest path from one node of a graph edge to the other,
        ///  or calculates it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="edge">The graph edge.</param>
        /// <returns>The shortest path from outside to inside, not containing either and
        /// ordered from inside to outside or outside to inside.</returns>
        public ushort[] GetShortestPath(GraphEdge edge)
        {
            return GetShortestPath(edge.outside, edge.inside);
        }

        /// <summary>
        ///  Retrieves the shortest path from one node to another, or calculates
        ///  it if it has not yet been found and CalculateFully has not been called.
        /// </summary>
        /// <param name="a">The first graph node.</param>
        /// <param name="b">The second graph node</param>
        /// <returns>The shortest path from a to b, not containing either and ordered from a to b or b to a.</returns>
        public ushort[] GetShortestPath(GraphNode a, GraphNode b)
        {
            var index = GetIndex(a, b);

            if (_gotFullyCached)
            {
                return _paths[index];
            }

            if (!_distances.ContainsKey(index))
            {
                Dijkstra(a, b);
            }
            return _paths[index];
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
            var aI = a.Id;
            var bI = b.Id;
            return (uint)(Math.Min(aI, bI) << 16) + Math.Max(aI, bI);
        }

        /// <summary>
        /// Calculates and caches all distances between the given nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <remarks>Calls to GetDistance and GetShortestPath after this method
        /// has been called must already be cached or exceptions will be thrown.</remarks>
        public void CalculateFully(GraphNode[] nodes)
        {
            _markedNodeCount = nodes.Length;

            foreach (var node in nodes)
            {
                Dijkstra(node);
            }

            _gotFullyCached = true;
        }

        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node until the target node is found (if specified) or until all marked nodes got checked.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The (optional) target node.</param>
        private void Dijkstra(GraphNode start, GraphNode target = null)
        {
            if (start == target) return;

            // The last newly found nodes.
            var front = new HashSet<GraphNode>() { start };
            // The already visited nodes.
            var visited = new HashSet<GraphNode>() { start };
            // The dictionary of the predecessors of the visited nodes.
            var predecessors = new Dictionary<ushort, ushort>();
            // The traversed distance from the starting node in edges.
            var distFromStart = 0;

            var nodesLeft = _markedNodeCount;
            if (start.Marked)
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
                        if (adjacentNode.Marked)
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

            if (target != null || nodesLeft > 0)
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

            _distances[index] = length;
            _paths[index] = path;
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

        internal class GraphNotConnectedException : Exception
        {
        }
    }
}
