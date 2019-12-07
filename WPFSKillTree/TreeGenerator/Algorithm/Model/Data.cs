namespace PoESkillTree.TreeGenerator.Algorithm.Model
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
        DistanceCalculator DistanceCalculator { get; }

        /// <summary>
        /// Gets the distance lookup where the Steiner bottleneck distances are stored.
        /// </summary>
        DistanceLookup SMatrix { get; }

        /// <summary>
        /// Gets or sets the index of the start node of the current reduced skill tree. Needs to be changed if the 
        /// start node is merged into another node, which becomes the new start node.
        /// </summary>
        int StartNodeIndex { get; set; }
    }

    /// <summary>
    /// Implementation of <see cref="IData"/>.
    /// </summary>
    public class Data : IData
    {
        public GraphEdgeSet EdgeSet { get; set; } = default!;
        public DistanceCalculator DistanceCalculator { get; set; } = default!;
        public DistanceLookup SMatrix { get; set; }

        /// <summary>
        /// Gets or sets the the start node of the current reduced skill tree.
        /// </summary>
        public GraphNode StartNode { get; private set; }

        public int StartNodeIndex
        {
            get => StartNode.DistancesIndex;
            set => StartNode = DistanceCalculator.IndexToNode(value);
        }

        public Data(GraphNode startNode)
        {
            StartNode = startNode;
        }
    }
}