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
        readonly Dictionary<uint, int> _distances;

        readonly Dictionary<uint, ushort[]> _paths;

        public DistanceLookup()
        {
            _distances = new Dictionary<uint, int>();
            _paths = new Dictionary<uint, ushort[]>();
        }

        /// <summary>
        ///  Retrieves the path distance from one node pf a graph edge to the other,
        ///  or calculates it if it has not yet been found.
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
        ///  it if it has not yet been found.
        /// </summary>
        /// <param name="a">The first graph node.</param>
        /// <param name="b">The second graph node</param>
        /// <returns>The length of the path from a to b (equals the amount of edges
        /// traversed).</returns>
        public int GetDistance(GraphNode a, GraphNode b)
        {
            var index = GetIndex(a, b);

            // If we already calculated the shortest path, use that.
            if (_distances.ContainsKey(index))
            {
                try
                {
                    return _distances[index];
                }
                catch (KeyNotFoundException)
                {
                    // In really rare cases this exception happens.
                    // Most likely because of resizing in the locked RunAndSaveDijkstra-method, so locking
                    // here too makes sure it doesn't happen again.
                    lock (this)
                    {
                        return _distances[index];
                    }
                }
                
            }

            return RunAndSaveDijkstra(a, b).Item1;
        }

        /// <summary>
        ///  Retrieves the shortest path from one node of a graph edge to the other,
        ///  or calculates it if it has not yet been found.
        /// </summary>
        /// <param name="edge">The graph edge.</param>
        /// <returns>The shortest path from outside to inside, not containing either and ordered from inside to outside.</returns>
        public ushort[] GetShortestPath(GraphEdge edge)
        {
            return GetShortestPath(edge.outside, edge.inside);
        }

        /// <summary>
        ///  Retrieves the shortest path from one node to another, or calculates
        ///  it if it has not yet been found.
        /// </summary>
        /// <param name="a">The first graph node.</param>
        /// <param name="b">The second graph node</param>
        /// <returns>The shortest path from a to b, not containing either and ordered from b to a.</returns>
        public ushort[] GetShortestPath(GraphNode a, GraphNode b)
        {
            var index = GetIndex(a, b);

            if (_paths.ContainsKey(index))
            {
                try
                {
                    return _paths[index];
                }
                catch (KeyNotFoundException)
                {
                    // See comment in GetDistance()
                    lock (this)
                    {
                        return _paths[index];
                    }
                }
            }

            return RunAndSaveDijkstra(a, b).Item2;
        }

        /// <summary>
        /// Runs Dijkstra on the parameters, saves the result tuple and returns it.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The target node.</param>
        /// <returns>A tuple containing the distance from the start node to the target node and the shortest path.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]// Most simple thread-safety approach, but seems to work.
        private Tuple<int, ushort[]> RunAndSaveDijkstra(GraphNode start, GraphNode target)
        {
            var index = GetIndex(start, target);
            if (_distances.ContainsKey(index))
                return new Tuple<int, ushort[]>(_distances[index], _paths[index]);

            var result = Dijkstra(start, target);

            _distances.Add(index, result.Item1);
            _paths.Add(index, result.Item2);

            return result;
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
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node until the target node is found.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The target node.</param>
        /// <returns>A tuple containing the distance from the start node to the target node and the shortest path.</returns>
        public Tuple<int, ushort[]> Dijkstra(GraphNode start, GraphNode target)
        {
            if (start == target) return new Tuple<int, ushort[]>(0, new ushort[0]);

            // The last newly found nodes.
            var front = new HashSet<GraphNode>() { start };
            // The already visited nodes.
            var visited = new HashSet<GraphNode>();
            // The dictionary of the predecessors of the visited nodes.
            var predecessors = new Dictionary<ushort, ushort>();
            // The traversed distance from the starting node in edges.
            var distFromStart = 0;

            // Doing this iterative because it's (theoretically) faster.
            while (true)
            {
                var newFront = new HashSet<GraphNode>();
                visited.UnionWith(front);

                foreach (var node in front)
                {
                    foreach (var adjacentNode in node.Adjacent)
                    {
                        if (adjacentNode == target)
                        {
                            var length = distFromStart + 1;
                            predecessors.Add(adjacentNode.Id, node.Id);
                            var path = GenerateShortestPath(start.Id, target.Id, predecessors, length);

                            return new Tuple<int, ushort[]>(length, path);
                        }

                        // newFront check is necessary because the dictionary complains about duplicates
                        if (visited.Contains(adjacentNode) || newFront.Contains(adjacentNode))
                            continue;

                        newFront.Add(adjacentNode);

                        predecessors.Add(adjacentNode.Id, node.Id);
                    }
                }

                if (newFront.Count <= 0) throw new GraphNotConnectedException();

                front = newFront;
                distFromStart = distFromStart + 1;
            }
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
