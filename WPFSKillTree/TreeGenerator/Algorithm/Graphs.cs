using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    ///  Abstract class representing a node (or a collection thereof) in the
    ///  simplified skill tree.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class GraphNode
    {
        private readonly ushort _id;
        public ushort Id { get { return _id; } }

#if DEBUG
        private string Name { get { return SkillTree.Skillnodes[_id].Name; } }
#endif

        public int DistancesIndex { get; set; }
        
        private List<GraphNode> _adjacent = new List<GraphNode>();
        public IReadOnlyCollection<GraphNode> Adjacent { get { return _adjacent;} }

        private readonly List<ushort> _nodes;
        public IReadOnlyCollection<ushort> Nodes { get { return _nodes; } }

        public int Size { get { return Nodes.Count; } }
        
        internal GraphNode(ushort id)
        {
            DistancesIndex = -1;
            _nodes = new List<ushort> {id};
            _id = id;
        }

        internal GraphNode(IEnumerable<ushort> nodes)
        {
            DistancesIndex = -1;
            _nodes = new List<ushort>(nodes);
            if (!_nodes.Any()) throw new ArgumentException("Node enumerable must not be empty", "nodes");
            _id = _nodes.First();
        }

        internal void AddNeighbor(GraphNode other)
        {
            if (!_adjacent.Contains(other))
            {
                _adjacent.Add(other);
            }
        }

        public void MergeWith(GraphNode other, IEnumerable<ushort> path)
        {
            _adjacent = _adjacent.Union(other._adjacent).Where(n => n != this && n != other).ToList();
            _nodes.AddRange(other._nodes);
            _nodes.AddRange(path);
            other._adjacent.Clear();
            other._nodes.Clear();
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
            var graphNode = new GraphNode(node.Id);
            NodeDict.Add(node, graphNode);
            CheckLinks(node);
            return graphNode;
        }

        public GraphNode AddNodeId(ushort nodeId)
        {
            return AddNode(SkillTree.Skillnodes[nodeId]);
        }

        public GraphNode SetStartNodes(HashSet<ushort> startNodes)
        {
            var supernode = new GraphNode(startNodes);
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

                    adjacentNode.AddNeighbor(currentNode);
                    currentNode.AddNeighbor(adjacentNode);
                }
            }
        }
    }

}