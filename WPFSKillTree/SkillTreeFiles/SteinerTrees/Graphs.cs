using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class GraphEdge : LinkedListPriorityQueueNode<GraphEdge>
    {
        public readonly GraphNode inside, outside;

        public GraphEdge(GraphNode inside, GraphNode outside)
        {
            this.inside = inside;
            this.outside = outside;
        }

    }

    /// <summary>
    ///  Abstract class representing a node (or a collection thereof) in the
    ///  simplified skill tree.
    /// </summary>
    public abstract class GraphNode
    {
        protected ushort id;
        public ushort Id { get { return id; } }

        public bool Marked { get; set; }

        public HashSet<GraphNode> Adjacent = new HashSet<GraphNode>();
    }

    /// <summary>
    ///  A graph node representing an actual node in the skill tree.
    /// </summary>
    public class SingleNode : GraphNode
    {
        public SkillNode baseNode;

        public SingleNode(SkillNode baseNode)
        {
            this.baseNode = baseNode;
            this.id = baseNode.Id;
        }
    }

    /// <summary>
    ///  A graph node representing a collection of nodes of the skill tree.
    ///  This is used to group up the already skilled nodes.
    /// </summary>
    public class Supernode : GraphNode
    {
        public HashSet<SkillNode> nodes = new HashSet<SkillNode>();

        public Supernode(HashSet<ushort> nodes)
        {
            foreach (ushort nodeId in nodes)
            {
                this.nodes.Add(SkillTree.Skillnodes[nodeId]);
            }
            // For lack of a better way.
            this.id = this.nodes.First().Id;
        }
    }

    /// <summary>
    /// A graph representing a simplified skill tree. 
    /// </summary>
    public class SearchGraph
    {
        public Dictionary<SkillNode, GraphNode> nodeDict;

        public SearchGraph()
        {
            nodeDict = new Dictionary<SkillNode, GraphNode>();
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
            nodeDict.Add(node, graphNode);
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
                nodeDict.Add(node, supernode);
                CheckLinks(node);
            }
            return supernode;
        }

        private void CheckLinks(SkillNode node)
        {
            if (!nodeDict.ContainsKey(node)) return;
            GraphNode currentNode = nodeDict[node];

            foreach (SkillNode neighbor in node.Neighbor)
            {
                if (nodeDict.ContainsKey(neighbor))
                {
                    GraphNode adjacentNode = nodeDict[neighbor];

                    if (adjacentNode == currentNode) continue;

                    adjacentNode.Adjacent.Add(currentNode);
                    currentNode.Adjacent.Add(adjacentNode);
                }
            }
        }
    }

}