using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class MinimalSpanningTree
    {
        private readonly HashSet<GraphNode> _mstNodes;

        private readonly DistanceLookup _distances;

        // I'd like to control at what point the spanning actually happens.
        private bool _isSpanned;
        public bool IsSpanned
        { get { return _isSpanned; } }

        public List<GraphEdge> SpanningEdges;

        private HashSet<ushort> _usedNodes;
        public HashSet<ushort> UsedNodes
        {
            get
            {
                if (_usedNodes == null)
                {
                    _usedNodes = new HashSet<ushort>();
                    foreach (var edge in SpanningEdges)
                    {
                        // Shortest paths are saved in DistanceLookup, so we can use those.
                        var path = _distances.GetShortestPath(edge);
                        // Save nodes into the HashSet, the set only saves each node once.
                        var inside = edge.inside as Supernode;
                        if (inside != null)
                        {
                            _usedNodes.UnionWith(inside.nodes.Select(node => node.Id));
                        }
                        var outside = edge.outside as Supernode;
                        if (outside != null)
                        {
                            _usedNodes.UnionWith(outside.nodes.Select(node => node.Id));
                        }
                        _usedNodes.Add(edge.inside.Id);
                        _usedNodes.Add(edge.outside.Id);
                        _usedNodes.UnionWith(path);
                    }
                }
                return _usedNodes;
            }
        }

        private int? _usedNodeCount;
        public int UsedNodeCount
        {
            get
            {
                if (_usedNodeCount == null)
                {
                    _usedNodeCount = UsedNodes.Count;
                }
                return _usedNodeCount.Value;
            }
        }

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned.</param>
        /// <param name="distances">An optional DistanceLookup parameter which
        /// caches the found node-node distances.</param>
        public MinimalSpanningTree(HashSet<GraphNode> mstNodes, DistanceLookup distances = null)
        {
            // Copy might be preferable, doesn't really matter atm though.
            _mstNodes = mstNodes;
            _distances = distances ?? new DistanceLookup();
            _isSpanned = false;
        }

        /// <summary>
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        /// </summary>
        /// <param name="startFrom">A GraphNode to start from.</param>
        /// <returns>A list of GraphEdges forming the MST.</returns>
        public List<GraphEdge> Span(GraphNode startFrom)
        {
            // With n nodes, we can have up to n (actually n-1) edges adjacent to each node.
            var adjacentEdgeQueue = new LinkedListPriorityQueue<GraphEdge>(100);
            // Removing all edges that satisfy a property (here a certain "outside"
            // node) from the queue is not actually trivial, since you could only
            // iterate over all entries (and you want to avoid that) if you don't
            // have the references to the edges at hand.
            // I guess this is the easiest way to do it...
            Dictionary<GraphNode, List<GraphEdge>> edgesLeadingToNode =
                new Dictionary<GraphNode, List<GraphEdge>>(_mstNodes.Count);
            foreach (GraphNode node in _mstNodes)
                edgesLeadingToNode[node] = new List<GraphEdge>(_mstNodes.Count);

            // All nodes that are already included.
            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            // All nodes that are not yet included.
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(_mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(startFrom);
            toAdd.Remove(startFrom);
            foreach (var otherNode in toAdd)
            {
                var adjacentEdge = new GraphEdge(startFrom, otherNode);
                adjacentEdgeQueue.Enqueue(adjacentEdge, _distances.GetDistance(startFrom, otherNode));
                edgesLeadingToNode[otherNode].Add(adjacentEdge);
            }

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                GraphEdge shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                GraphNode newIn = shortestEdge.outside;

                inMst.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                foreach (var obsoleteEdge in edgesLeadingToNode[newIn])
                {
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }
                edgesLeadingToNode.Remove(newIn);

                // Find all newly adjacent edges and enqueue them.
                foreach (var otherNode in toAdd)
                {
                    var edge = new GraphEdge(newIn, otherNode);
                    adjacentEdgeQueue.Enqueue(edge, _distances.GetDistance(newIn, otherNode));
                    edgesLeadingToNode[otherNode].Add(edge);
                }
            }
            if (toAdd.Count > 0)
                throw new DistanceLookup.GraphNotConnectedException();

            SpanningEdges = mstEdges;
            _isSpanned = true;
            return mstEdges;
        }
    }
}
