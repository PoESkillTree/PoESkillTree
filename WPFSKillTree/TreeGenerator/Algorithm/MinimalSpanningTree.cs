using System;
using System.Buffers;
using System.Collections.Generic;

namespace PoESkillTree.TreeGenerator.Algorithm
{
    /// <summary>
    ///     Provides algorithms for building the minimal spanning distance tree between a set of nodes
    ///     while saving the spanning edges.
    /// </summary>
    public class MinimalSpanningTree : IDisposable
    {
        private readonly DistanceLookup _distances;

        private readonly IReadOnlyList<int> _mstNodes;

        private PooledList<DirectedGraphEdge> _spanningEdges;

        /// <summary>
        ///     Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned. (not null)</param>
        /// <param name="distances">The DistanceLookup used as cache.</param>
        public MinimalSpanningTree(IReadOnlyList<int> mstNodes, DistanceLookup distances)
        {
            _mstNodes = mstNodes ?? throw new ArgumentNullException(nameof(mstNodes));
            _distances = distances;
        }

        /// <summary>
        ///     Gets the edges which span this tree as a list of <see cref="DirectedGraphEdge" />s.
        ///     Only set after a Span-method has been called.
        /// </summary>
        public IReadOnlyList<DirectedGraphEdge> SpanningEdges => _spanningEdges;

        /// <summary>
        ///     Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///     O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startIndex">The node index to start from.</param>
        public void Span(int startIndex)
        {
            var notIncluded = ArrayPool<int>.Shared.Rent(_mstNodes.Count);
            var notIncludedCount = 0;

            var isIncluded = ArrayPool<bool>.Shared.Rent(_distances.CacheSize);
            Array.Clear(isIncluded, 0, isIncluded.Length);

            _spanningEdges?.Dispose();
            _spanningEdges = new PooledList<DirectedGraphEdge>(_mstNodes.Count);

            using (var adjacentEdgeQueue = new LinkedListPriorityQueue<DirectedGraphEdge>(100, _mstNodes.Count*_mstNodes.Count))
            {
                InitializeDataStructures(startIndex, notIncluded, ref notIncludedCount, adjacentEdgeQueue);
                isIncluded[startIndex] = true;

                while (notIncludedCount > 0 && !adjacentEdgeQueue.IsEmpty)
                {
                    int newIn;
                    DirectedGraphEdge shortestEdge;
                    // Dequeue and ignore edges that are already inside the MST.
                    // Add the first one that is not.
                    do
                    {
                        shortestEdge = adjacentEdgeQueue.Dequeue();
                        newIn = shortestEdge.Outside;
                    } while (isIncluded[newIn]);

                    _spanningEdges.Add(shortestEdge);
                    isIncluded[newIn] = true;

                    // Find all newly adjacent edges and enqueue them.
                    IncludeNode(notIncluded, ref notIncludedCount, adjacentEdgeQueue, newIn);
                }
            }

            ArrayPool<bool>.Shared.Return(isIncluded);
            ArrayPool<int>.Shared.Return(notIncluded);
        }

        private void InitializeDataStructures(
            int startIndex, int[] notIncluded, ref int notIncludedCount,
            LinkedListPriorityQueue<DirectedGraphEdge> adjacentEdgeQueue)
        {
            for (var i = 0; i < _mstNodes.Count; i++)
            {
                var t = _mstNodes[i];
                if (t != startIndex)
                {
                    notIncluded[notIncludedCount] = t;
                    notIncludedCount++;
                    var distance = _distances[startIndex, t];
                    adjacentEdgeQueue.Enqueue(new DirectedGraphEdge(startIndex, t), distance);
                }
            }
        }

        private void IncludeNode(
            int[] notIncluded, ref int notIncludedCount,
            LinkedListPriorityQueue<DirectedGraphEdge> adjacentEdgeQueue,
            int node)
        {
            for (var i = 0; i < notIncludedCount; i++)
            {
                var otherNode = notIncluded[i];
                if (otherNode == node)
                {
                    RemoveAt(notIncluded, ref notIncludedCount, i);
                    i--;
                }
                else
                {
                    var distance = _distances[node, otherNode];
                    adjacentEdgeQueue.Enqueue(new DirectedGraphEdge(node, otherNode), distance);
                }
            }
        }

        private static void RemoveAt(int[] array, ref int arrayCount, int index)
        {
            arrayCount--;
            if (index < arrayCount)
            {
                Array.Copy(array, index + 1, array, index, arrayCount - index);
            }
        }

        /// <summary>
        ///     Uses Kruskal's algorithm to build an MST spanning the mstNodes
        ///     with the given edges as a list ordered by priority ascending.
        ///     The edges don't need to contain only nodes given to this instance
        ///     via constructor.
        ///     O(|edges|) runtime.
        /// </summary>
        /// <param name="orderedEdges">Edges ordered by priority ascending.</param>
        /// <remarks>
        ///     Both Span methods have quadratic runtime in the graph nodes. This one
        ///     has a lower constant factor but needs to filter out unneeded edges (quadratic
        ///     in all nodes), the other one doesn't need to do any filtering (quadratic in
        ///     considered nodes) so if the mst nodes are generally only a very small
        ///     portion of all nodes, use the other Span method, if not, use this one.
        /// </remarks>
        public void Span(IEnumerable<DirectedGraphEdge> orderedEdges)
        {
            _spanningEdges = new PooledList<DirectedGraphEdge>(_mstNodes.Count);
            var set = new DisjointSet(_distances.CacheSize);
            var considered = new bool[_distances.CacheSize];
            var toAddCount = _mstNodes.Count - 1;
            foreach (var t in _mstNodes)
            {
                considered[t] = true;
            }
            foreach (var current in orderedEdges)
            {
                var inside = current.Inside;
                var outside = current.Outside;
                // This condition is by far the bottleneck of the method.
                // (most likely because branch prediction can't predict the result)
                if (!considered[inside] | !considered[outside]) continue;
                if (set.Find(inside) == set.Find(outside)) continue;
                _spanningEdges.Add(current);
                set.Union(inside, outside);
                if (--toAddCount == 0) break;
            }
        }

        public void Dispose()
        {
            _spanningEdges?.Dispose();
        }
    }
}
