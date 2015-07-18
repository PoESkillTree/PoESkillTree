using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;
using System.Diagnostics;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class MinimalSpanningTree
    {
        public HashSet<GraphNode> mstNodes;

        private DistanceLookup distances;

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
                        var path = distances.GetShortestPath(edge);
                        // Save nodes into the HashSet, the set only saves each node once.
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
            this.mstNodes = mstNodes;
            this.distances = (distances == null ? new DistanceLookup() : distances);
            _isSpanned = false;
        }

        /// <summary>
        ///  Uses Prim's algorithm to build an MST spanning the mstNodes.
        /// </summary>
        /// <param name="startFrom">A GraphNode to start from.</param>
        /// <returns>A list of GraphEdges forming the MST.</returns>
        public List<GraphEdge> Span(GraphNode startFrom)
        {
            /// With n nodes, we can have up to n (actually n-1) edges adjacent to each node.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);
            /// Removing all edges that satisfy a property (here a certain "outside"
            /// node) from the queue is not actually trivial, since you could only
            /// iterate over all entries (and you want to avoid that) if you don't
            /// have the references to the edges at hand.
            /// I guess this is the easiest way to do it...
            Dictionary<GraphNode, List<GraphEdge>> edgesLeadingToNode =
                new Dictionary<GraphNode, List<GraphEdge>>();
            foreach (GraphNode node in mstNodes)
                edgesLeadingToNode[node] = new List<GraphEdge>();

            // All nodes that are already included.
            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            // All nodes that are not yet included.
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(startFrom);
            toAdd.Remove(startFrom);
            edgesLeadingToNode[startFrom] = new List<GraphEdge>();
            foreach (GraphNode otherNode in toAdd)
            {
                GraphEdge adjacentEdge = new GraphEdge(startFrom, otherNode);
                adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                edgesLeadingToNode[otherNode].Add(adjacentEdge);
            }

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                GraphEdge shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                GraphNode newIn = shortestEdge.outside;

                
                //if (inMst.Contains(newIn)) throw new Exception();
                //if (!toAdd.Contains(newIn)) throw new Exception("No edge to this node should remain!");

                inMst.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                foreach (GraphEdge obsoleteEdge in edgesLeadingToNode[newIn])
                {
                    //if (!inMst.Contains(obsoleteEdge.inside)) throw new Exception("This edge's inside node is not inside");
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }
                edgesLeadingToNode.Remove(newIn);

                // Find all newly adjacent edges and enqueue them.
                foreach (GraphNode otherNode in toAdd)
                {
                    GraphEdge adjacentEdge = new GraphEdge(newIn, otherNode);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                    edgesLeadingToNode[otherNode].Add(adjacentEdge);
                }
            }
            if (toAdd.Count > 0)
                throw new DistanceLookup.GraphNotConnectedException();

            this.SpanningEdges = mstEdges;
            _isSpanned = true;
            return mstEdges;
        }
    }
}
