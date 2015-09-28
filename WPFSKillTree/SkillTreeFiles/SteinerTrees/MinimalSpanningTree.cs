using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class MinimalSpanningTree
    {
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
        private bool _isSpanned;
        public bool IsSpanned
        { get { return _isSpanned; } }

        public List<GraphEdge> SpanningEdges { get; private set; }

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
                        var path = _distances.GetShortestPath(edge.Inside, edge.Outside);
                        // Save nodes into the HashSet, the set only saves each node once.
                        _usedNodes.Add(edge.Inside.Id);
                        _usedNodes.Add(edge.Outside.Id);
                        _usedNodes.UnionWith(path);
                    }
                }
                return new HashSet<ushort>(_usedNodes);
            }
        }

        /// <summary>
        ///  Instantiates a new MinimalSpanningTree.
        /// </summary>
        /// <param name="mstNodes">The GraphNodes that should be spanned.</param>
        /// <param name="distances">An optional DistanceLookup parameter which
        /// caches the found node-node distances.</param>
        public MinimalSpanningTree(List<GraphNode> mstNodes, DistanceLookup distances = null)
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
        public void Span(GraphNode startFrom)
        {
            var adjacentEdgeQueue = new LinkedListPriorityQueue<QueueNode>(100);

            var startIndex = startFrom.DistancesIndex;
            // All nodes that are not yet included.
            var toAdd = new List<int>(_mstNodes.Count);
            // Removing all edges that satisfy a property (here a certain "outside"
            // node) from the queue is not actually trivial, since you could only
            // iterate over all entries (and you want to avoid that) if you don't
            // have the references to the edges at hand.
            // I guess this is the easiest way to do it...
            var edgesLeadingToNode = new List<QueueNode>[_distances.CacheSize];
            // The spanning edges.
            var mstEdges = new List<QueueNode>();
            
            for (var i = 0; i < _mstNodes.Count; i++)
            {
                var index = _mstNodes[i].DistancesIndex;
                edgesLeadingToNode[index] = new List<QueueNode>(_mstNodes.Count);

                if (index != startIndex)
                {
                    toAdd.Add(index);

                    var adjacentEdge = new QueueNode(startIndex, index);
                    adjacentEdgeQueue.Enqueue(adjacentEdge, _distances[startIndex, index]);
                    edgesLeadingToNode[index].Add(adjacentEdge);
                }
            }
            
            while (toAdd.Count > 0 && adjacentEdgeQueue.Count > 0)
            {
                var shortestEdge = adjacentEdgeQueue.Dequeue();
                mstEdges.Add(shortestEdge);
                var newIn = shortestEdge.Outside;
                
                //toAdd.Remove(newIn);

                // Remove all edges that are entirely inside the MST now.
                for (var i = 0; i < edgesLeadingToNode[newIn].Count; i++)
                {
                    adjacentEdgeQueue.Remove(edgesLeadingToNode[newIn][i]);
                }

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
                        edgesLeadingToNode[otherNode].Add(edge);
                    }
                }
            }
            if (toAdd.Count > 0)
                throw new DistanceLookup.GraphNotConnectedException();

            SpanningEdges = new List<GraphEdge>();
            foreach (var queueNode in mstEdges)
            {
                SpanningEdges.Add(new GraphEdge(
                    _distances.IndexToNode(queueNode.Inside),
                    _distances.IndexToNode(queueNode.Outside)));
            }
            _isSpanned = true;
        }
    }
}
