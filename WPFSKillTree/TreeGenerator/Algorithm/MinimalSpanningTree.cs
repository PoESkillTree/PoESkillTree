using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Type of nodes used for the Priority Queue.
    /// The nodes of this edge are represented by the value of <see cref="GraphNode.DistancesIndex"/>.
    /// </summary>
    public class LinkedGraphEdge : LinkedListPriorityQueueNode<LinkedGraphEdge>
    {
        public readonly int Inside, Outside;

        public LinkedGraphEdge(int inside, int outside)
        {
            Inside = inside;
            Outside = outside;
        }
    }

    [DebuggerDisplay("{N1}-{N2}:{Weight}")]
    public class GraphEdge : IEquatable<GraphEdge>
    {
        public readonly int N1, N2;

        public readonly uint Weight;

        internal GraphEdge(int n1, int n2, uint weight)
        {
            N1 = Math.Min(n1, n2);
            N2 = Math.Max(n1, n2);
            Weight = weight;
        }

        public bool Equals(GraphEdge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return N1 == other.N1 && N2 == other.N2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GraphEdge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (N1 * 397) ^ N2;
            }
        }
    }

    public class MinimalSpanningTreeWithNodes : MinimalSpanningTree, IDisposable
    {
        private static readonly ConcurrentStack<HashSet<ushort>> HashSetStack = new ConcurrentStack<HashSet<ushort>>();

        private readonly List<HashSet<ushort>> _usedSets = new List<HashSet<ushort>>();

        private readonly IReadOnlyList<GraphNode> _mstGraphNodes;

        private readonly IDistancePathLookup _paths;

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned. (not null)</param>
        /// <param name="distances">The DistanceLookup used as cache. (not null)</param>
        public MinimalSpanningTreeWithNodes(IReadOnlyList<GraphNode> mstNodes, IDistancePathLookup distances)
            : base(mstNodes.Select(n => n.DistancesIndex).ToList(), distances)
        {
            _mstGraphNodes = mstNodes;
            _paths = distances;
        }

        /// <summary>
        /// Returns the nodes the spanning tree includes.
        /// </summary>
        public HashSet<ushort> GetUsedNodes()
        {
            if (SpanningEdges == null)
                throw new InvalidOperationException("MST is not spanned!");
            HashSet<ushort> hashSet;
            if (!HashSetStack.TryPop(out hashSet))
            {
                hashSet = new HashSet<ushort>();
            }
            hashSet.UnionWith(_mstGraphNodes.Select(n => n.Id));
            foreach (var edge in SpanningEdges)
            {
                hashSet.UnionWith(_paths.GetShortestPath(edge.Inside, edge.Outside));
            }
            _usedSets.Add(hashSet);
            return hashSet;
        }

        /// <summary>
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///  O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startFrom">A GraphNode to start from.</param>
        public void Span(GraphNode startFrom)
        {
            Span(startFrom.DistancesIndex);
        }

        public void Dispose()
        {
            foreach (var set in _usedSets)
            {
                set.Clear();
                HashSetStack.Push(set);
            }
        }
    }

    public class MinimalSpanningTree
    {

        private readonly IReadOnlyList<int> _mstNodes;

        private readonly IDistanceLookup _distances;

        public IReadOnlyList<LinkedGraphEdge> SpanningEdges { get; private set; }

        /// <summary>
        /// Returns the edges which span this tree as an linq enumerable (so it's slow and not cached).
        /// Only set after <see cref="Span(GraphNode)"/> or <see cref="Span(LinkedGraphEdge)"/> has been called.
        /// </summary>
        public IEnumerable<GraphEdge> SpanningGraphEdges
        {
            get
            {
                return SpanningEdges.Select(e => new GraphEdge(e.Inside, e.Outside, _distances[e.Inside, e.Outside]));
            }
        }

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
                    var adjacentEdge = new LinkedGraphEdge(startIndex, t);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, _distances[startIndex, t]);
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
                        var edge = new LinkedGraphEdge(newIn, otherNode);
                        adjacentEdgeQueue.Enqueue(edge, _distances[newIn, otherNode]);
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
