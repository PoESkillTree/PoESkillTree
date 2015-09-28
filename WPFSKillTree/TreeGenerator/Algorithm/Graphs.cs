using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public class GraphEdge
    {
        public readonly GraphNode Inside, Outside;

        public GraphEdge(GraphNode inside, GraphNode outside)
        {
            Inside = inside;
            Outside = outside;
        }

    }

    /// <summary>
    ///  Abstract class representing a node (or a collection thereof) in the
    ///  simplified skill tree.
    /// </summary>
    public abstract class GraphNode
    {
        private readonly ushort _id;
        public ushort Id { get { return _id; } }

        public int DistancesIndex { get; set; }

        public readonly HashSet<GraphNode> Adjacent = new HashSet<GraphNode>();

        protected GraphNode(ushort id)
        {
            _id = id;
            DistancesIndex = -1;
        }
    }

    /// <summary>
    ///  A graph node representing an actual node in the skill tree.
    /// </summary>
    public class SingleNode : GraphNode
    {
        public SingleNode(SkillNode baseNode)
            : base(baseNode.Id)
        { }
    }

    /// <summary>
    ///  A graph node representing a collection of nodes of the skill tree.
    ///  This is used to group up the already skilled nodes.
    /// </summary>
    public class Supernode : GraphNode
    {
        public readonly HashSet<SkillNode> Nodes = new HashSet<SkillNode>();

        public Supernode(HashSet<ushort> nodes)
            : base(nodes.First())
        {
            foreach (ushort nodeId in nodes)
            {
                Nodes.Add(SkillTree.Skillnodes[nodeId]);
            }
        }
    }

    /// <summary>
    /// A graph representing a simplified skill tree. 
    /// </summary>
    public class SearchGraph
    {
        public readonly Dictionary<SkillNode, GraphNode> NodeDict;

        public SearchGraph()
        {
            NodeDict = new Dictionary<SkillNode, GraphNode>();
        }

        /// <summary>
        ///  Adds a skill node to the graph. New nodes are automatically
        ///  connected to existing adjacent nodes.
        /// </summary>
        /// <param name="node">The skill node to be added.</param>
        /// <returns>The graph node that is added to the graph.</returns>
        public GraphNode AddNode(SkillNode node)
        {
            SingleNode graphNode = new SingleNode(node);
            NodeDict.Add(node, graphNode);
            CheckLinks(node);
            return graphNode;
        }

        public GraphNode AddNodeId(ushort nodeId)
        {
            return AddNode(SkillTree.Skillnodes[nodeId]);
        }

        public Supernode SetStartNodes(HashSet<ushort> startNodes)
        {
            Supernode supernode = new Supernode(startNodes);
            foreach (ushort nodeId in startNodes)
            {
                SkillNode node = SkillTree.Skillnodes[nodeId];
                NodeDict.Add(node, supernode);
                CheckLinks(node);
            }
            return supernode;
        }

        private void CheckLinks(SkillNode node)
        {
            if (!NodeDict.ContainsKey(node)) return;
            GraphNode currentNode = NodeDict[node];

            foreach (SkillNode neighbor in node.Neighbor)
            {
                if (NodeDict.ContainsKey(neighbor))
                {
                    GraphNode adjacentNode = NodeDict[neighbor];

                    if (adjacentNode == currentNode) continue;

                    adjacentNode.Adjacent.Add(currentNode);
                    currentNode.Adjacent.Add(adjacentNode);
                }
            }
        }
    }

}