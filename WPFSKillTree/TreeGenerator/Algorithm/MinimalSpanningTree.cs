using System;
using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Provides algorithms for building the minimal spanning distance tree between a set of nodes
    /// while saving the spanning edges.
    /// </summary>
    public class MinimalSpanningTree
    {
        private readonly IReadOnlyList<int> _mstNodes;

        private readonly IDistanceLookup _distances;

        /// <summary>
        /// Gets the edges which span this tree as a list of <see cref="LinkedGraphEdge"/>s.
        /// Only set after <see cref="Span(int)"/> or <see cref="Span(LinkedGraphEdge)"/> has been called.
        /// </summary>
        public IReadOnlyList<LinkedGraphEdge> SpanningEdges { get; private set; }

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
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
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///  O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startIndex">The node index to start from.</param>
        public void Span(int startIndex)
        {
            var adjacentEdgeQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);

            // All nodes that are not yet included.
            var toAdd = new List<int>(_mstNodes.Count);
            // If the index node is already included.
            var inMst = new bool[_distances.CacheSize];
            // The spanning edges.
            var mstEdges = new List<LinkedGraphEdge>(_mstNodes.Count);

            foreach (var t in _mstNodes)
            {
                if (t != startIndex)
                {
                    toAdd.Add(t);
                    var adjacentEdge = new LinkedGraphEdge(startIndex, t, _distances[startIndex, t]);
                    adjacentEdgeQueue.Enqueue(adjacentEdge);
                }
            }
            inMst[startIndex] = true;

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                int newIn;
                LinkedGraphEdge shortestEdge;
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
                        var edge = new LinkedGraphEdge(newIn, otherNode, _distances[newIn, otherNode]);
                        adjacentEdgeQueue.Enqueue(edge);
                    }
                }
            }

            SpanningEdges = mstEdges;
        }

        /// <summary>
        /// Uses Kruskal's algorithm to build an MST spanning the mstNodes
        /// with the given edges as a linked list ordered by priority ascending.
        /// The edges don't need to contain only nodes given to this instance
        /// via constructor.
        /// O(|edges|) runtime.
        /// </summary>
        /// <param name="first">First edge of the linked list.</param>
        /// <remarks>
        /// Both Span methods have quadratic runtime in the graph nodes. This one
        /// has a lower constant factor but needs to filter out unneeded edges (quadratic
        /// in all nodes), the other one doesn't need to do any filtering (quadratic in
        /// considered nodes) so if the mst nodes are generally only a very small
        /// portion of all nodes, use the other Span method, if not, use this one.
        /// </remarks>
        public void Span(LinkedGraphEdge first)
        {
            var mstEdges = new List<LinkedGraphEdge>(_mstNodes.Count);
            var set = new DisjointSet(_distances.CacheSize);
            var considered = new bool[_distances.CacheSize];
            var toAddCount = _mstNodes.Count - 1;
            foreach (var t in _mstNodes)
            {
                considered[t] = true;
            }
            for (var current = first; current != null; current = current.Next)
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
