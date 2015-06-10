using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("UnitTests")]
namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  Calculates and caches distances between nodes. Only relies on adjacency
    ///  information stored in the nodes.
    /// </summary>
    //[assembly: InternalsVisibleTo("UnitTests")]
    class DistanceLookup
    {
        // The uint compounds both ushort indices.
        Dictionary<uint, int> _distances;

        public DistanceLookup()
        {
            _distances = new Dictionary<uint, int>();
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
            uint index = getIndex(a, b);

            // If we already calculated the shortest path, use that.
            if (_distances.ContainsKey(index))
                return _distances[index];

            // Otherwise, use pathfinding to find it...
            int pathLength = Dijkstra(a, b);
            //... and save it...
            _distances.Add(index, pathLength);
            // ...and return it.
            return pathLength;
        }

        private void setDistance(GraphNode a, GraphNode b, int distance)
        {
            uint index = getIndex(a, b);
            if (!_distances.ContainsKey(index))
                _distances.Add(index, distance);
        }

        /// <summary>
        ///  Compounds two ushort node indices into a single uint one, which
        ///  is independent of the order of the two indices.
        /// </summary>
        /// <param name="a">The first index.</param>
        /// <param name="b">The second index.</param>
        /// <returns>The compounded index.</returns>
        private uint getIndex(GraphNode a, GraphNode b)
        {
            ushort aI = a.Id;
            ushort bI = b.Id;
            return (uint)(Math.Min(aI, bI) << 16) + (uint)(Math.Max(aI, bI));
        }

        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node until the target node is found.
        ///  All visited nodes have their distance from the start node updated.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The target node.</param>
        /// <returns>The distance from the start node to the target node.</returns>
        public int Dijkstra(GraphNode start, GraphNode target)
        {
            if (start == target) return 0;
            return dijkstraStep(start, target, new HashSet<GraphNode>() { start },
                new HashSet<GraphNode>(), 0);
        }


        /// <summary>
        ///  Uses a djikstra-like algorithm to flood the graph from the start
        ///  node until the target node is found.
        ///  All visited nodes have their distance from the start node updated.
        /// </summary>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The target node.</param>
        /// <param name="front">The last newly found nodes.</param>
        /// <param name="visited">The already visited nodes.</param>
        /// <param name="distFromStart">The traversed distance from the
        /// starting node in edges.</param>
        /// <returns>The distance from the start node to the target node.</returns>
        /// <remarks> - Currently the target node is never found if contained
        /// in front or visited.
        ///  - If front = { start }, then distFromStart should be 0.</remarks>
        public int dijkstraStep(GraphNode start, GraphNode target,
            HashSet<GraphNode> front, HashSet<GraphNode> visited, int distFromStart)
        {
            HashSet<GraphNode> newFront = new HashSet<GraphNode>();
            HashSet<GraphNode> newVisited = new HashSet<GraphNode>(visited);
            newVisited.Concat(front);

            foreach (GraphNode node in front)
            {
                newVisited.Add(node);
                foreach (GraphNode adjacentNode in node.Adjacent)
                {
                    if (adjacentNode == target) return distFromStart + 1;

                    // Could be combined in newVisited...
                    if (visited.Contains(adjacentNode)) continue;
                    if (front.Contains(adjacentNode)) continue;

                    newFront.Add(adjacentNode);
                }
            }
            // This wouldn't need recursion, but it's more convenient this way.
            if (newFront.Count > 0)
                return dijkstraStep(start, target, newFront, newVisited, distFromStart + 1);
            throw new GraphNotConnectedException();
        }

        internal class GraphNotConnectedException : Exception
        {
        }
    }
}
