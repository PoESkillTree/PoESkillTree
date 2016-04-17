using System;
using System.Collections.Generic;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    ///     Provides algorithms for building the minimal spanning distance tree between a set of nodes
    ///     while saving the spanning edges.
    /// </summary>
    public class MinimalSpanningTree
    {
        private readonly IDistanceLookup _distances;

        private readonly IReadOnlyList<int> _mstNodes;

        /// <summary>
        ///     Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned. (not null)</param>
        /// <param name="distances">The DistanceLookup used as cache. (not null)</param>
        public MinimalSpanningTree(IReadOnlyList<int> mstNodes, IDistanceLookup distances)
        {
            if (mstNodes == null) throw new ArgumentNullException("mstNodes");
            if (distances == null) throw new ArgumentNullException("distances");

            _mstNodes = mstNodes;
            _distances = distances;
        }

        /// <summary>
        ///     Gets the edges which span this tree as a list of <see cref="DirectedGraphEdge" />s.
        ///     Only set after a Span-method has been called.
        /// </summary>
        public IReadOnlyList<DirectedGraphEdge> SpanningEdges { get; private set; }

        /// <summary>
        ///     Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///     O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startIndex">The node index to start from.</param>
        public void Span(int startIndex)
        {
            var edges = new FunctionalTwoDArray<DirectedGraphEdge>((a, b) => new DirectedGraphEdge(a, b, _distances[a, b]));
            Span(startIndex, edges);
        }

        /// <summary>
        ///     Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///     O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startIndex">The node index to start from.</param>
        /// <param name="edges">Cache for the edges used.</param>
        public void Span(int startIndex, ITwoDArray<DirectedGraphEdge> edges)
        {
            // All nodes that are not yet included.
            var toAdd = new List<int>(_mstNodes.Count);
            // If the index node is already included.
            var inMst = new bool[_distances.CacheSize];
            // The spanning edges.
            var mstEdges = new List<DirectedGraphEdge>(_mstNodes.Count);

            using (var adjacentEdgeQueue = new LinkedListPriorityQueue<DirectedGraphEdge>(100, _mstNodes.Count*_mstNodes.Count))
            {
                foreach (var t in _mstNodes)
                {
                    if (t != startIndex)
                    {
                        toAdd.Add(t);
                        adjacentEdgeQueue.Enqueue(edges[startIndex, t]);
                    }
                }
                inMst[startIndex] = true;

                while (toAdd.Count > 0 && !adjacentEdgeQueue.IsEmpty)
                {
                    int newIn;
                    DirectedGraphEdge shortestEdge;
                    // Dequeue and ignore edges that are already inside the MST.
                    // Add the first one that is not.
                    do
                    {
                        shortestEdge = adjacentEdgeQueue.Dequeue();
                        newIn = shortestEdge.Outside;
                    } while (inMst[newIn]);
                    mstEdges.Add(shortestEdge);
                    inMst[newIn] = true;

                    // Find all newly adjacent edges and enqueue them.
                    for (var i = 0; i < toAdd.Count; i++)
                    {
                        var otherNode = toAdd[i];
                        if (otherNode == newIn)
                        {
                            toAdd.RemoveAt(i--);
                        }
                        else
                        {
                            adjacentEdgeQueue.Enqueue(edges[newIn, otherNode]);
                        }
                    }
                }
            }

            SpanningEdges = mstEdges;
        }

        /// <summary>
        ///     Uses Kruskal's algorithm to build an MST spanning the mstNodes
        ///     with the given edges as a list ordered by priority ascending.
        ///     The edges don't need to contain only nodes given to this instance
        ///     via constructor.
        ///     O(|edges|) runtime.
        /// </summary>
        /// <param name="ordererdEdges">Edges ordered by priority ascending.</param>
        /// <remarks>
        ///     Both Span methods have quadratic runtime in the graph nodes. This one
        ///     has a lower constant factor but needs to filter out unneeded edges (quadratic
        ///     in all nodes), the other one doesn't need to do any filtering (quadratic in
        ///     considered nodes) so if the mst nodes are generally only a very small
        ///     portion of all nodes, use the other Span method, if not, use this one.
        /// </remarks>
        public void Span(IEnumerable<DirectedGraphEdge> ordererdEdges)
        {
            var mstEdges = new List<DirectedGraphEdge>(_mstNodes.Count);
            var set = new DisjointSet(_distances.CacheSize);
            var considered = new bool[_distances.CacheSize];
            var toAddCount = _mstNodes.Count - 1;
            foreach (var t in _mstNodes)
            {
                considered[t] = true;
            }
            foreach (var current in ordererdEdges)
            {
                var inside = current.Inside;
                var outside = current.Outside;
                // This condition is by far the bottleneck of the method.
                // (most likely because branch prediction can't predict the result)
                if (!considered[inside] | !considered[outside]) continue;
                if (set.Find(inside) == set.Find(outside)) continue;
                mstEdges.Add(current);
                set.Union(inside, outside);
                if (--toAddCount == 0) break;
            }
            SpanningEdges = mstEdges;
        }
    }
}
