using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Tuple<float, double>[] _statConstraints;
        private string[] _statConstraintNames;
        private Dictionary<string, int> _statNameLookup;

        private Dictionary<int, List<Tuple<int, float>>> _nodeStats;
        private Dictionary<int, bool> _areTravelNodes;

        private float[] _fixedStats;

        private HashSet<ushort> _fixedNodes;

        protected override GeneticAlgorithmParameters GaParameters
        {
            get
            {   // TODO adjust
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
            FormalizeConstraints(Settings.StatConstraints);
            
            // Extract stats from nodes and set travel nodes.
            var skillNodes = Settings.SubsetTree.Count > 0
                ? Settings.SubsetTree.ToDictionary(id => id, id => SkillTree.Skillnodes[id])
                : SkillTree.Skillnodes;
            _nodeStats = new Dictionary<int, List<Tuple<int, float>>>(skillNodes.Count);
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
                    where _statNameLookup.ContainsKey(attr.Key)
                    select new Tuple<int, float>(_statNameLookup[attr.Key], attr.Value[0]))
                    .ToList();
                _nodeStats[id] = nodeAttributes;

                if (nodeAttributes.Count > 0)
                {
                }

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

            CreateSearchGraph();
        }

        private void FormalizeConstraints(Dictionary<string, Tuple<float, double>> statConstraints)
        {
            _statConstraints = new Tuple<float, double>[statConstraints.Count];
            _statConstraintNames = new string[statConstraints.Count];
            _statNameLookup = new Dictionary<string, int>(statConstraints.Count);
            _fixedStats = new float[statConstraints.Count];
            var i = 0;
            foreach (var kvPair in statConstraints)
            {
                _statConstraints[i] = kvPair.Value;
                _statConstraintNames[i] = kvPair.Key.Replace("#", @"[0-9]*\.?[0-9]+").Replace("+", @"+").Replace(".", @".");
                _statNameLookup.Add(kvPair.Key, i);
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
                        && _nodeStats[node.Id].Count > 0 && !_areTravelNodes[node.Id])
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

                        SearchGraph.AddNode(node);
                    }
                }
            }

            // Add potential steiner nodes. TODO some kind of vicinity related selection
            // Add all non-travel nodes with stats. TODO exclude nodes with stats that are "too far away" to be useful
            SearchSpace = new List<GraphNode>(SearchGraph.nodeDict.Values.Where(
                node => node != StartNodes && !TargetNodes.Contains(node)
                    && (node.Adjacent.Count > 2 || (_nodeStats[node.Id].Count > 0 && !_areTravelNodes[node.Id]))));
        }

        protected override MinimalSpanningTree FilterSearchSpace()
        {
            // Don't add nodes that are not connected to the start node (through cross-tagging)
            SearchSpace = SearchSpace.Where(IsConnected).ToList();

            // LeastSolution: MST between start and check-tagged nodes.
            var nodes = new HashSet<GraphNode>(TargetNodes) { StartNodes };
            var leastSolution = new MinimalSpanningTree(nodes, Distances);
            leastSolution.Span(StartNodes);

            // Set start stats from start and target nodes.
            AddStats(_fixedNodes, _fixedStats);
            // Add the initial stats from the settings.
            foreach (var initialStat in Settings.InitialStats)
            {
                if (_statNameLookup.ContainsKey(initialStat.Key))
                {
                    var tuple = new Tuple<int, float>(_statNameLookup[initialStat.Key], initialStat.Value);
                    AddStat(tuple, _fixedStats);
                }
            }

#if DEBUG
            var selectedTravelNodes = 0;
            var selectedStatNodes = 0;
            var selectedOtherNodes = 0;
            foreach (var node in SearchSpace)
            {
                if (_areTravelNodes[node.Id])
                {
                    selectedTravelNodes++;
                }
                else if (_nodeStats[node.Id].Count > 0)
                {
                    selectedStatNodes++;
                }
                else
                {
                    selectedOtherNodes++;
                }
            }
            Debug.WriteLine("Travel nodes: " + selectedTravelNodes);
            Debug.WriteLine("Stat nodes: " + selectedStatNodes);
            Debug.WriteLine("Other nodes: " + selectedOtherNodes);
#endif

            return leastSolution;
        }

        private void AddStats(IEnumerable<ushort> ids, IList<float> to)
        {
            foreach (var id in ids)
            {
                foreach (var tuple in _nodeStats[id])
                {
                    AddStat(tuple, to);
                }
            }
        }

        private static void AddStat(Tuple<int, float> stat, IList<float> to)
        {
            to[stat.Item1] += stat.Item2;
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
            var totalStats = (float[])_fixedStats.Clone();
            var usedNodes = tree.UsedNodes;
            // Don't count the character start node.
            var usedNodeCount = tree.UsedNodeCount - 1;
            usedNodes.ExceptWith(_fixedNodes);
            AddStats(usedNodes, totalStats);

            // Calculate constraint value for each stat and multiply them.
            var csvs = 1.0;
            var i = 0;
            foreach (var stat in _statConstraints)
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
            // TODO Different scaling for values exceeding target value?
            x = Math.Min(x, target);
            return Math.Exp(weight*10 * x/target) / Math.Exp(weight*10);
        }
    }
}