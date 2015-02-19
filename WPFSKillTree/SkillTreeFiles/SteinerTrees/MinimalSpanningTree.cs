using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;
using System.Diagnostics;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    class MinimalSpanningTree
    {
        public HashSet<GraphNode> mstNodes;

        public List<GraphEdge> SpanningEdges;

        DistanceLookup distances;

        public MinimalSpanningTree(HashSet<GraphNode> mstNodes, DistanceLookup distances = null)
        {
            // Copy might be preferable, doesn't really matter atm though.
            this.mstNodes = mstNodes;

            this.distances = (distances == null ? new DistanceLookup() : distances);
        }

        public List<GraphEdge> Span(GraphNode startFrom)
        {
            // We will have at most one adjacent edge to each node, so that's our limit.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);
            /// Removing all edges that satisfy a property (here a certain "outside"
            /// node) from the queue is not actually trivial, since you could only
            /// iterate over all entries (and you want to avoid that) if you don't
            /// have the references to the edges at hand.
            /// I guess this is the easiest way to do it...
            Dictionary<GraphNode, List<GraphEdge>> edgesToNode = new Dictionary<GraphNode, List<GraphEdge>>();
            foreach (GraphNode node in mstNodes)
                edgesToNode[node] = new List<GraphEdge>();

            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(startFrom);
            toAdd.Remove(startFrom);
            edgesToNode[startFrom] = new List<GraphEdge>();
            foreach (GraphNode otherNode in toAdd)
            {
                GraphEdge adjacentEdge = new GraphEdge(startFrom, otherNode);
                adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                edgesToNode[otherNode].Add(adjacentEdge);
            }

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                GraphEdge shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                GraphNode newIn = shortestEdge.outside;


                if (inMst.Contains(newIn)) throw new Exception();
                if (!toAdd.Contains(newIn)) throw new Exception("No edge to this node should remain!");

                inMst.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                foreach (GraphEdge obsoleteEdge in edgesToNode[newIn])
                {
                    if (!inMst.Contains(obsoleteEdge.inside)) throw new Exception("This edge's inside node is not inside");
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }
                edgesToNode.Remove(newIn);

                // Find all newly adjacent edges and enqueue them.
                foreach (GraphNode otherNode in toAdd)
                {
                    GraphEdge adjacentEdge = new GraphEdge(newIn, otherNode);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                    edgesToNode[otherNode].Add(adjacentEdge);
                }
            }
            if (toAdd.Count > 0)
                throw new DistanceLookup.GraphNotConnectedException();

            this.SpanningEdges = mstEdges;
            return mstEdges;
        }


        private int? _usedNodeCount;
        public int UsedNodeCount
        {
            get
            {
                if (_usedNodeCount == null) 
                {
                    _usedNodeCount = 0;
                    foreach (GraphEdge edge in SpanningEdges)
                    {
                        _usedNodeCount += distances.GetDistance(edge);
                    }
                }
                return _usedNodeCount.Value;
            }
        }
    }
}
