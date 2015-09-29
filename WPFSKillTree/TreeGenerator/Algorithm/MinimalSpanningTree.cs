using System;
using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public class MinimalSpanningTree
    {
        /// <summary>
        /// Type of nodes used for the Priority Queue in Span.
        /// The nodes of this edge are represented by the value of <see cref="GraphNode.DistancesIndex"/>.
        /// </summary>
        private class QueueNode : LinkedListPriorityQueueNode<QueueNode>
        {
            public readonly int Inside, Outside;

            public QueueNode(int inside, int outside)
            {
                Inside = inside;
                Outside = outside;
            }
        }

        private readonly List<GraphNode> _mstNodes;

        private readonly DistanceLookup _distances;

        // I'd like to control at what point the spanning actually happens.
        public bool IsSpanned { get; private set; }

        /// <summary>
        /// Returns the edges which span this tree.
        /// Only set after <see cref="Span"/> has been called.
        /// </summary>
        public List<GraphEdge> SpanningEdges { get; private set; }

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned. (not null)</param>
        /// <param name="distances">An optional DistanceLookup parameter which
        /// caches the found node-node distances.</param>
        public MinimalSpanningTree(List<GraphNode> mstNodes, DistanceLookup distances = null)
        {
            if (mstNodes == null) throw new ArgumentNullException("mstNodes");

            // Copy might be preferable, doesn't really matter atm though.
            _mstNodes = mstNodes;
            _distances = distances ?? new DistanceLookup();
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
                if (path == null)
                    throw new GraphNotConnectedException();
                // Save nodes into the HashSet, the set only saves each node once.
                nodes.Add(edge.Inside.Id);
                nodes.Add(edge.Outside.Id);
                nodes.UnionWith(path);
            }
            return nodes;
        }

        /// <summary>
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        /// </summary>
        /// <param name="startFrom">A GraphNode to start from.</param>
        public void Span(GraphNode startFrom)
        {
            var adjacentEdgeQueue = new LinkedListPriorityQueue<QueueNode>(100);

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
                    var adjacentEdge = new QueueNode(startIndex, index);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, _distances[startIndex, index]);
                }
            }
            inMst[startIndex] = true;

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                int newIn;
                QueueNode shortestEdge;
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
                        var edge = new QueueNode(newIn, otherNode);
                        adjacentEdgeQueue.Enqueue(edge, _distances[newIn, otherNode]);
                    }
                }
            }
            if (toAdd.Count > 0)
                throw new GraphNotConnectedException();

            SpanningEdges = mstEdges;
            IsSpanned = true;
        }
    }
}
