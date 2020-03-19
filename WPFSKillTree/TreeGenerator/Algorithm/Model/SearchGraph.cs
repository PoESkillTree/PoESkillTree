using System.Collections.Generic;
using System.Linq;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.ViewModels.PassiveTree;

namespace PoESkillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    /// A graph representing a simplified skill tree. 
    /// </summary>
    public class SearchGraph
    {
        public readonly Dictionary<PassiveNodeViewModel, GraphNode> NodeDict;

        public SearchGraph()
        {
            NodeDict = new Dictionary<PassiveNodeViewModel, GraphNode>();
        }

        /// <summary>
        ///  Adds a skill node to the graph. New nodes are automatically
        ///  connected to existing adjacent nodes.
        /// </summary>
        /// <param name="node">The skill node to be added.</param>
        /// <returns>The graph node that is added to the graph.</returns>
        public GraphNode AddNode(PassiveNodeViewModel node)
        {
            var graphNode = new GraphNode(node.Id);
            NodeDict.Add(node, graphNode);
            CheckLinks(node);
            return graphNode;
        }

        public GraphNode AddNodeId(ushort nodeId)
        {
            return AddNode(SkillTree.Skillnodes[nodeId]);
        }

        public GraphNode SetStartNodes(IReadOnlyCollection<PassiveNodeViewModel> startNodes)
        {
            var supernode = new GraphNode(startNodes.Select(n => n.Id));
            foreach (var node in startNodes)
            {
                NodeDict.Add(node, supernode);
                CheckLinks(node);
            }
            return supernode;
        }

        private void CheckLinks(PassiveNodeViewModel node)
        {
            if (!NodeDict.ContainsKey(node)) return;
            GraphNode currentNode = NodeDict[node];

            foreach (var neighbor in node.NeighborPassiveNodes.Values)
            {
                if (NodeDict.ContainsKey(neighbor))
                {
                    GraphNode adjacentNode = NodeDict[neighbor];

                    if (adjacentNode == currentNode) continue;

                    adjacentNode.AddNeighbor(currentNode);
                    currentNode.AddNeighbor(adjacentNode);
                }
            }
        }
    }
}