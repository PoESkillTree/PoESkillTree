using System;
using System.Collections.Generic;

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

    public class MinimalSpanningTree
    {

        private readonly List<GraphNode> _mstNodes;

        private readonly DistanceLookup _distances;

        // I'd like to control at what point the spanning actually happens.
        public bool IsSpanned { get; private set; }

        /// <summary>
        /// Returns the edges which span this tree.
        /// Only set after <see cref="Span(GraphNode)"/> or <see cref="Span(LinkedGraphEdge)"/> has been called.
        /// </summary>
        public List<GraphEdge> SpanningEdges { get; private set; }

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned. (not null)</param>
        /// <param name="distances">The DistanceLookup used as cache.
        /// <see cref="DistanceLookup.CalculateFully"/> must have been called. (not null</param>
        public MinimalSpanningTree(List<GraphNode> mstNodes, DistanceLookup distances)
        {
            if (mstNodes == null) throw new ArgumentNullException("mstNodes");
            if (distances == null) throw new ArgumentNullException("distances");
            if (!distances.FullyCached) throw new ArgumentException("CalculateFully must have been called.", "distances");

            // Copy might be preferable, doesn't really matter atm though.
            _mstNodes = mstNodes;
            _distances = distances;
            IsSpanned = false;
        }

        /// <summary>
        /// Returns the nodes the spanning tree includes.
        /// </summary>
        public HashSet<ushort> GetUsedNodes()
        {
            var nodes = new HashSet<ushort>();
            foreach (var edge in SpanningEdges)
            {
                // Shortest paths are saved in DistanceLookup, so we can use those.
                var path = _distances.GetShortestPath(edge.Inside, edge.Outside);
                // Save nodes into the HashSet, the set only saves each node once.
                nodes.Add(edge.Inside.Id);
                nodes.Add(edge.Outside.Id);
                nodes.UnionWith(path);
            }
            return nodes;
        }

        /// <summary>
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        ///  O(|mstNodes|^2) runtime.
        /// </summary>
        /// <param name="startFrom">A GraphNode to start from.</param>
        public void Span(GraphNode startFrom)
        {
            var adjacentEdgeQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);

            var startIndex = startFrom.DistancesIndex;
            // All nodes that are not yet included.
            var toAdd = new List<int>(_mstNodes.Count);
            // If the index node is already included.
            var inMst = new bool[_distances.CacheSize];
            // The spanning edges.
            var mstEdges = new List<GraphEdge>(_mstNodes.Count);

            for (var i = 0; i < _mstNodes.Count; i++)
            {
                var index = _mstNodes[i].DistancesIndex;
                if (index != startIndex)
                {
                    toAdd.Add(index);
                    var adjacentEdge = new LinkedGraphEdge(startIndex, index);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, _distances[startIndex, index]);
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
                mstEdges.Add(new GraphEdge(
                    _distances.IndexToNode(shortestEdge.Inside),
                    _distances.IndexToNode(shortestEdge.Outside)));
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
            IsSpanned = true;
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
            var mstEdges = new List<GraphEdge>(_mstNodes.Count);
            var set = new DisjointSet(_distances.CacheSize);
            var considered = new bool[_distances.CacheSize];
            var toAddCount = _mstNodes.Count - 1;
            foreach (var t in _mstNodes)
            {
                considered[t.DistancesIndex] = true;
            }
            for (var current = first; current != null; current = current.Next)
            {
                var inside = current.Inside;
                var outside = current.Outside;
                // This condition is by far the bottleneck of the method.
                // (most likely because branch prediction can't predict the result)
                if (!considered[inside] | !considered[outside]) continue;
                if (set.Find(inside) == set.Find(outside)) continue;
                mstEdges.Add(new GraphEdge(_distances.IndexToNode(inside), _distances.IndexToNode(outside)));
                set.Union(inside, outside);
                if (--toAddCount == 0) break;
            }
            SpanningEdges = mstEdges;
            IsSpanned = true;
        }
    }
}
