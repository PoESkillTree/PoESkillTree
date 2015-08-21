using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public class AdvancedSolver : AbstractSolver<AdvancedSolverSettings>
    {
        private static readonly Regex TravelNodeRegex = new Regex(@"\+# to (Strength|Intelligence|Dexterity)");

        private Tuple<float, double>[] _attrConstraints;
        private string[] _attrConstraintNames;
        private Dictionary<string, int> _attrNameLookup;

        private Dictionary<int, List<Tuple<int, float>>> _nodeAttributes;
        private Dictionary<int, bool> _areTravelNodes;

        private float[] _fixedAttributes;

        private HashSet<ushort> _fixedNodes;

        protected override GeneticAlgorithmParameters GaParameters
        {
            get
            {
                return new GeneticAlgorithmParameters(
                    (int)(0.3 * (SearchSpace.Count < 150 ? 20000.0 / SearchSpace.Count : SearchSpace.Count)),
                    (int)(2.5 * SearchSpace.Count),
                    SearchSpace.Count, 6, 1);
            }
        }

        public AdvancedSolver(SkillTree tree, AdvancedSolverSettings settings) : base(tree, settings)
        {
        }

        protected override void BuildSearchGraph()
        {
            // Assign a number to each StatConstraint.
            FormalizeConstraints(Settings.AttributeConstraints);
            
            // Extract stats from nodes and set travel nodes.
            var skillNodes = Settings.SubsetTree.Count > 0
                ? Settings.SubsetTree.ToDictionary(id => id, id => SkillTree.Skillnodes[id])
                : SkillTree.Skillnodes;
            _nodeAttributes = new Dictionary<int, List<Tuple<int, float>>>(skillNodes.Count);
            _areTravelNodes = new Dictionary<int, bool>(skillNodes.Count);
            foreach (var node in skillNodes)
            {
                var id = node.Key;
                var skillNode = node.Value;

                // Remove stats that have no constraints.
                // Replace stats that have constraints with a tuple of their number and the value.
                // For attributes with more than one value, the first one is selected,
                // that is reasonable for the attributes the skill tree currently has.
                // Attributes without value are not supported, if a constraint without value slips
                // through, it will break.
                var nodeAttributes = 
                    (from attr in skillNode.Attributes
                    where _attrNameLookup.ContainsKey(attr.Key)
                    select new Tuple<int, float>(_attrNameLookup[attr.Key], attr.Value[0]))
                    .ToList();
                _nodeAttributes[id] = nodeAttributes;

                // Set if the node is a travel node.
                if (skillNode.Attributes.Count == 1 && TravelNodeRegex.IsMatch(skillNode.Attributes.Keys.First())
                    && skillNode.Attributes.Values.First().Any(v => (int)v == 10))
                {
                    _areTravelNodes[id] = true;
                }
                else
                {
                    _areTravelNodes[id] = false;
                }
            }

            SearchGraph = new SearchGraph();

            // Add start and check-tagged nodes as in SteinerSolver.
            CreateStartNodes();
            CreateTargetNodes();
            _fixedNodes = new HashSet<ushort>(StartNodes.nodes.Select(node => node.Id));
            _fixedNodes.UnionWith(TargetNodes.Select(node => node.Id));
            CreateFixedAttributes();

            CreateSearchGraph();
        }

        private void FormalizeConstraints(Dictionary<string, Tuple<float, double>> statConstraints)
        {
            _attrConstraints = new Tuple<float, double>[statConstraints.Count];
            _attrConstraintNames = new string[statConstraints.Count];
            _attrNameLookup = new Dictionary<string, int>(statConstraints.Count);
            _fixedAttributes = new float[statConstraints.Count];
            var i = 0;
            foreach (var kvPair in statConstraints)
            {
                _attrConstraints[i] = kvPair.Value;
                _attrConstraintNames[i] = kvPair.Key.Replace("#", @"[0-9]*\.?[0-9]+").Replace("+", @"+").Replace(".", @".");
                _attrNameLookup.Add(kvPair.Key, i);
                i++;
            }
        }

        private void CreateStartNodes()
        {
            if (Settings.SubsetTree.Count > 0 || Settings.InitialTree.Count > 0)
            {
                // if the current tree does not need to be part of the result, only skill the character node
                StartNodes = SearchGraph.SetStartNodes(new HashSet<ushort> { Tree.GetCharNodeId() });
            }
            else
            {
                StartNodes = SearchGraph.SetStartNodes(Tree.SkilledNodes);
            }
        }

        private void CreateTargetNodes()
        {
            TargetNodes = new HashSet<GraphNode>();
            foreach (var nodeId in Settings.Checked)
            {
                // Don't add nodes that are already skilled.
                if (SearchGraph.nodeDict.ContainsKey(SkillTree.Skillnodes[nodeId]))
                    continue;
                // Don't add nodes that should not be skilled.
                if (Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(nodeId))
                    continue;
                // Add target node to the graph.
                var node = SearchGraph.AddNodeId(nodeId);
                TargetNodes.Add(node);
            }
        }

        private void CreateSearchGraph()
        {
            // TODO somewhat merge with SteinerSolver to reduce redundancy
            foreach (SkillNodeGroup ng in SkillTree.NodeGroups)
            {
                bool mustInclude = false;

                SkillNode firstNeighbor = null;

                // Find out if this node group can be omitted.
                foreach (SkillNode node in ng.Nodes)
                {
                    // If the group contains a skilled node or a target node,
                    // it can't be omitted.
                    if (SearchGraph.nodeDict.ContainsKey(node))
                    {
                        mustInclude = true;
                        break;
                    }

                    // If the node has stats and is not a travel node and is part of the subtree,
                    // the group is included.
                    if ((Settings.SubsetTree.Count == 0 || Settings.SubsetTree.Contains(node.Id))
                        && _nodeAttributes[node.Id].Count > 0 && !_areTravelNodes[node.Id])
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
                        if (SearchGraph.nodeDict.ContainsKey(node))
                            continue;
                        // Don't add nodes that should not be skilled.
                        if (Settings.Crossed.Contains(node.Id))
                            continue;
                        // Only add nodes in the subsettree if one is given.
                        if (Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(node.Id))
                            continue;
                        // Mastery nodes are obviously not useful.
                        if (node.IsMastery)
                            continue;
                        // Keystones can only be included if they are check-tagged.
                        if (node.IsKeyStone)
                            continue;

                        SearchGraph.AddNode(node);
                    }
                }
            }
        }

        private void CreateFixedAttributes()
        {
            // Set start stats from start and target nodes.
            AddAttributes(_fixedNodes, _fixedAttributes);
            // Add the initial stats from the settings.
            foreach (var initialStat in Settings.InitialAttributes)
            {
                if (_attrNameLookup.ContainsKey(initialStat.Key))
                {
                    var tuple = new Tuple<int, float>(_attrNameLookup[initialStat.Key], initialStat.Value);
                    AddAttribute(tuple, _fixedAttributes);
                }
            }
        }

        protected override bool IncludeNode(GraphNode node)
        {
            // Add potential steiner nodes. TODO some kind of vicinity related selection
            // Add all non-travel nodes with stats. TODO exclude nodes with stats that are "too far away" to be useful
            return node != StartNodes && !TargetNodes.Contains(node)
                   && (node.Adjacent.Count > 2 || (_nodeAttributes[node.Id].Count > 0 && !_areTravelNodes[node.Id]));
        }

        protected override MinimalSpanningTree CreateLeastSolution()
        {
            // LeastSolution: MST between start and check-tagged nodes.
            var nodes = new HashSet<GraphNode>(TargetNodes) { StartNodes };
            var leastSolution = new MinimalSpanningTree(nodes, Distances);
            leastSolution.Span(StartNodes);
            return leastSolution;
        }

        protected override bool IncludeNodeUsingDistances(GraphNode node)
        {
            // Don't add nodes that are not connected to the start node (through cross-tagging)
            return IsConnected(node);
        }

        private void AddAttributes(IEnumerable<ushort> ids, IList<float> to)
        {
            foreach (var id in ids)
            {
                foreach (var tuple in _nodeAttributes[id])
                {
                    AddAttribute(tuple, to);
                }
            }
        }

        private static void AddAttribute(Tuple<int, float> attrTuple, IList<float> to)
        {
            to[attrTuple.Item1] += attrTuple.Item2;
        }

        private bool IsConnected(GraphNode node)
        {
            // Going with try-catch is probably not the best approach, but it
            // doesn't require refactoring DistanceLookup and the exception
            // case should not appear often enough to influence performance.
            try
            {
                Distances.GetDistance(node, StartNodes);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        protected override double FitnessFunction(MinimalSpanningTree tree)
        {
            // Add stats of the MST-nodes and start stats.
            var totalStats = (float[])_fixedAttributes.Clone();
            var usedNodes = tree.UsedNodes;
            // Don't count the character start node.
            var usedNodeCount = tree.UsedNodeCount - 1;
            usedNodes.ExceptWith(_fixedNodes);
            AddAttributes(usedNodes, totalStats);

            // Calculate constraint value for each stat and multiply them.
            var csvs = 1.0;
            var i = 0;
            foreach (var stat in _attrConstraints)
            {
                csvs *= CalcCsv(totalStats[i], stat.Item2, stat.Item1);
                i++;
            }

            // Total points spent is another csv.
            if (usedNodeCount > Settings.TotalPoints)
            {
                // If UsedNodeCount is higher than Settings.TotalPoints, it is 
                // calculated as a csv with a weight of 5. (and lower = better)
                csvs *= CalcCsv(2 * Settings.TotalPoints - usedNodeCount, 5, Settings.TotalPoints);
            }
            else
            {
                // TODO test this factor, might not be optimal
                // optimal: least points spent with csv product = 1 (all constraints satisfied)
                // That means: minimize UsedNodeCount until Settings.TotalPoints > maximize csvs > minimize UsedNodeCount

                // If it is lower, apply it as a logarithmic factor.
                csvs *= 1 + 0.5 * Math.Log(Settings.TotalPoints + 1 - usedNodeCount);
            }

            return csvs;
        }

        private static double CalcCsv(float x, double weight, float target)
        {
            // Don't go higher than the target value.
            // TODO how to handle csvs exceeding the target value? (curently they are capped at the target value)
            x = Math.Min(x, target);
            return Math.Exp(weight*10 * x/target) / Math.Exp(weight*10);
        }
    }
}