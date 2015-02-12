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
            // Copy might be preferable.
            this.mstNodes = mstNodes;

            this.distances = (distances == null ? new DistanceLookup() : distances);
        }

        public List<GraphEdge> Span(GraphNode startFrom)
        {
            // We will have at most one adjacent edge to each node.
            HeapPriorityQueue<GraphEdge> adjacentEdgeQueue = new HeapPriorityQueue<GraphEdge>(mstNodes.Count * mstNodes.Count);
            // I guess this is the easiest way to do it...
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
                //if (newIn.Name == "Savagery") Debugger.Break();
                //if (newIn.Name == "Constitution") Debugger.Break();
                //if (newIn.Name == "Bravery") Debugger.Break();


                if (inMst.Contains(newIn)) throw new Exception();
                if (!toAdd.Contains(newIn)) throw new Exception("No edge to this node should remain!");

                inMst.Add(newIn);
                toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                foreach (GraphEdge obsoleteEdge in edgesToNode[newIn])
                {
                    if (!inMst.Contains(obsoleteEdge.inside)) throw new Exception("This edge's inside node is not inside");
                    //if (obsoleteEdge.inside.Name == "Savagery") Debugger.Break();
                    //if (obsoleteEdge.inside.Name == "Constitution") Debugger.Break();
                    adjacentEdgeQueue.Remove(obsoleteEdge);
                }
                edgesToNode.Remove(newIn);

                // Find all newly adjacent edges and enqueue them.
                foreach (GraphNode otherNode in toAdd)
                {
                    GraphEdge adjacentEdge = new GraphEdge(newIn, otherNode);
                    //if (otherNode.Name == "Savagery") Debugger.Break();
                    //if (otherNode.Name == "Constitution") Debugger.Break();
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
                    // TODO: Check for off-by-one.
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
