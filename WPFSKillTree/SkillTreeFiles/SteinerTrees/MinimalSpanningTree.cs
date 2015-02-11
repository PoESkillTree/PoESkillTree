using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    class MinimalSpanningTree
    {
        public HashSet<GraphNode> mstNodes;

        public List<GraphEdge> SpanningEdges;

        DistanceLookup distances;

        public MinimalSpanningTree(HashSet<GraphNode> mstNodes, DistanceLookup distances = null)
        {
            // Copy might be preferable.
            this.mstNodes = mstNodes;

            this.distances = (distances == null ? new DistanceLookup() : distances);
        }

        public List<GraphEdge> Span(GraphNode startFrom)
        {
            // We will have at most one adjacent edge to each node.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);

            HashSet<GraphNode> inMst = new HashSet<GraphNode>();
            HashSet<GraphNode> toAdd = new HashSet<GraphNode>(mstNodes);

            List<GraphEdge> mstEdges = new List<GraphEdge>();

            // Initialize the MST with the start nodes.
            inMst.Add(startFrom);
            toAdd.Remove(startFrom);

            foreach (GraphNode otherNode in toAdd)
            {
                GraphEdge adjacentEdge = new GraphEdge(startFrom, otherNode);
                // Priority is set to negative distance.
                adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
            }

            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                GraphEdge shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                GraphNode newIn = shortestEdge.outside;

                if (inMst.Contains(newIn)) throw new Exception();

                mstNodes.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                foreach (GraphEdge obsoleteEdge in adjacentEdgeQueue.Where(e => e.outside == newIn))
                {
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }

                // Find all newly adjacent edges and enqueue them.
                foreach (GraphNode otherNode in toAdd)
                {
                    GraphEdge adjacentEdge = new GraphEdge(newIn, otherNode);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, distances.GetDistance(adjacentEdge));
                }
            }
            if (toAdd.Count > 0)
                throw new GraphNotConnectedException();

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
                    // TODO: Check for off-by-one.
                    foreach (GraphEdge edge in SpanningEdges)
                    {
                        _usedNodeCount += distances.GetDistance(edge);
                    }
                }
                return _usedNodeCount.Value;
            }
        }


        internal class GraphNotConnectedException : Exception
        {
        }
    }
}
