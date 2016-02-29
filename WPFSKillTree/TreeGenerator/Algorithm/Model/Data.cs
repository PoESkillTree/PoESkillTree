using System;

namespace POESKillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    /// Interface used to provide the reductions in SteinerReductions with the necessary data.
    /// </summary>
    public interface IData
    {
        /// <summary>
        /// Gets the edge set storing the current edges of the reduced skill tree.
        /// </summary>
        GraphEdgeSet EdgeSet { get; }

        /// <summary>
        /// Gets the distance lookup where the distances between the nodes of the reduced skill tree are stored.
        /// </summary>
        DistanceLookup DistanceLookup { get; }

        /// <summary>
        /// Gets the distance lookup where the Steiner bottleneck distances are stored.
        /// </summary>
        IDistanceLookup SMatrix { get; }

        /// <summary>
        /// Gets or sets the start node of the current reduced skill tree. Needs to be changed if the 
        /// start node is merged into another node, which becomes the new start node.
        /// </summary>
        GraphNode StartNode { get; set; }
    }

    /// <summary>
    /// Implementation of <see cref="IData"/>. Provides an Event for changes to <see cref="StartNode"/>.
    /// </summary>
    public class Data : IData
    {
        public GraphEdgeSet EdgeSet { get; set; }
        public DistanceLookup DistanceLookup { get; private set; }
        public IDistanceLookup SMatrix { get; set; }

        private GraphNode _startNode;

        public GraphNode StartNode
        {
            get { return _startNode; }
            set
            {
                if (_startNode == value) return;
                _startNode = value;
                if (StartNodeChanged != null)
                    StartNodeChanged(this, _startNode);
            }
        }

        public Data(GraphEdgeSet edgeSet, DistanceLookup distanceLookup, GraphNode startNode)
        {
            EdgeSet = edgeSet;
            DistanceLookup = distanceLookup;
            _startNode = startNode;
        }

        /// <summary>
        /// Event which is raised after <see cref="StartNode"/> was changed.
        /// </summary>
        public event EventHandler<GraphNode> StartNodeChanged;
    }
}