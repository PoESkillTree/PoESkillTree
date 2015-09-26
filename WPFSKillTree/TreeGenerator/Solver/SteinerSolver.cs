using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public class SteinerSolver : AbstractSolver<SolverSettings>
    {
        private const double GenMultiplier = 0.3;

        private const double PopMultiplier = 1.5;

        private const int ConstRuntimeEndpoint = 150;

        private int _maxEdgeDistance;

        protected override GeneticAlgorithmParameters GaParameters
        {
            // TODO to test: depth of tree as parameter for maxGeneration and populationSize (more depth => harder?)
            // something like average distance of target nodes to each other?
            // up to ConstRuntimeEndpoint: maxGeneration * populationSize = GenMultiplier * PopMultiplier * ConstRuntimeEndpoint^2
            // after that: maxGeneration * populationSize = GenMultiplier * PopMultiplier * SearchSpace.Count^2
            get
            {
                return new GeneticAlgorithmParameters(
                    SearchSpace.Count == 0 ? 0
                        : (int)(GenMultiplier * (SearchSpace.Count < ConstRuntimeEndpoint ? (ConstRuntimeEndpoint*ConstRuntimeEndpoint) / SearchSpace.Count : SearchSpace.Count)),
                    (int)(PopMultiplier * SearchSpace.Count),
                    SearchSpace.Count, 6, 1);
            }
        }

        public SteinerSolver(SkillTree tree, SolverSettings settings)
            : base(tree, settings)
        {
        }

        protected override void BuildSearchGraph()
        {
            SearchGraph = new SearchGraph();

            // Add the start nodes to the graph.
            StartNodes = SearchGraph.SetStartNodes(Tree.SkilledNodes);

            TargetNodes = new HashSet<GraphNode>();
            // Add the target nodes to the graph.
            foreach (ushort nodeId in Settings.Checked)
            {
                // Don't add nodes that are already skilled.
                if (SearchGraph.NodeDict.ContainsKey(SkillTree.Skillnodes[nodeId]))
                    continue;
                // Add target node to the graph.
                GraphNode node = SearchGraph.AddNodeId(nodeId);
                TargetNodes.Add(node);
            }

            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool mustInclude = false;

                SkillNode firstNeighbor = null;

                // Find out if this node group can be omitted.
                foreach (SkillNode node in ng.Nodes)
                {
                    // If the group contains a skilled node or a target node,
                    // it can't be omitted.
                    if (SearchGraph.NodeDict.ContainsKey(node))
                    {
                        mustInclude = true;
                        break;
                    }

                    // If the group is adjacent to more than one node, it must
                    // also be fully included (since it's not isolated and could
                    // be part of a path to other nodes).
                    var ng1 = ng;
                    foreach (SkillNode neighbor in node.Neighbor.Where(neighbor => neighbor.SkillNodeGroup != ng1))
                    {
                        if (firstNeighbor == null)
                            firstNeighbor = neighbor;

                        // Does the group have more than one neighbor?
                        if (neighbor != firstNeighbor)
                        {
                            mustInclude = true;
                            break;
                        }
                    }
                    if (mustInclude) break;
                }

                if (mustInclude)
                {
                    // Add the group's nodes individually
                    foreach (SkillNode node in ng.Nodes)
                    {
                        // Can't path through class starts.
                        if (SkillTree.rootNodeList.Contains(node.Id))
                            continue;
                        // Don't add nodes that are already in the graph (as
                        // target or start nodes).
                        if (SearchGraph.NodeDict.ContainsKey(node))
                            continue;
                        // Don't add nodes that should not be skilled.
                        if (Settings.Crossed.Contains(node.Id))
                            continue;

                        SearchGraph.AddNode(node);
                    }
                }
            }
        }

        protected override bool IncludeNode(GraphNode node)
        {
            // Steiner nodes need to have at least 2 neighbors.
            return node.Adjacent.Count > 2 && node != StartNodes && !TargetNodes.Contains(node);
        }

        protected override MinimalSpanningTree CreateLeastSolution()
        {
            var nodes = new List<GraphNode>(TargetNodes) { StartNodes };
            MinimalSpanningTree leastSolution = new MinimalSpanningTree(nodes, Distances);
            leastSolution.Span(StartNodes);

            if (TargetNodes.Count == 0)
            {
                _maxEdgeDistance = -1;
            }
            else
            {
                _maxEdgeDistance = leastSolution.SpanningEdges.Max(edge => Distances[edge.Inside, edge.Outside]);
            }

            return leastSolution;
        }

        protected override bool IncludeNodeUsingDistances(GraphNode node)
        {
            // Find potential steiner points that are in reasonable vicinity.
            return _maxEdgeDistance >= 0 && TargetNodes.Any(targetNode => Distances[targetNode, node] < _maxEdgeDistance);
        }

        protected override double FitnessFunction(MinimalSpanningTree tree)
        {
            return 1500 - tree.UsedNodeCount;
        }
    }
}