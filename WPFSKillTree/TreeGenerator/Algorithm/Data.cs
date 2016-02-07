using System;
using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public interface IData
    {
        GraphEdgeSet EdgeSet { get; }

        IReadOnlyList<GraphNode> SearchSpace { get; }

        DistanceLookup DistanceLookup { get; }

        IDistanceLookup SMatrix { get; }

        GraphNode StartNode { get; set; }
    }

    public class Data : IData
    {
        public GraphEdgeSet EdgeSet { get; set; }
        public IReadOnlyList<GraphNode> SearchSpace { get; set; }
        public DistanceLookup DistanceLookup { get; private set; }
        public IDistanceLookup SMatrix { get; set; }

        private GraphNode _startNode;

        public GraphNode StartNode
        {
            get { return _startNode; }
            set
            {
                _startNode = value;
                if (StartNodeChanged != null)
                    StartNodeChanged(this, _startNode);
            }
        }

        public Data(GraphEdgeSet edgeSet, IReadOnlyList<GraphNode> searchSpace, DistanceLookup distanceLookup, GraphNode startNode)
        {
            EdgeSet = edgeSet;
            SearchSpace = searchSpace;
            DistanceLookup = distanceLookup;
            _startNode = startNode;
        }

        public event EventHandler<GraphNode> StartNodeChanged;
    }
}