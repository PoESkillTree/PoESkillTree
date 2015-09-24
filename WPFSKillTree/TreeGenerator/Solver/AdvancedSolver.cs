using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public class AdvancedSolver : AbstractSolver<AdvancedSolverSettings>
    {
        private class ConvertedPseudoAttribute
        {
            public List<Tuple<string, float>> Attributes { get; private set; }

            public Tuple<float, double> TargetWeightTuple { get; private set; }

            public ConvertedPseudoAttribute(List<Tuple<string, float>> attributes, Tuple<float, double> tuple)
            {
                Attributes = attributes;
                TargetWeightTuple = tuple;
            }
        }

        private static readonly Regex ContainsWildcardRegex = new Regex(@"{\d}");

        private const double GenMultiplier = 0.4;

        private const double PopMultiplier = 5;

        private const int ConstRuntimeEndpoint = 150;

        /// <summary>
        /// Weight of the CSV of the used node count if it is higher than the allowed node count.
        /// </summary>
        private const double UsedNodeCountWeight = 5;

        /// <summary>
        /// Factor for the value calculated from the node difference if used node count is lower than the allowed node coutn.
        /// </summary>
        private const double UsedNodeCountFactor = .01;

        /// <summary>
        /// Factor by which weights get multiplied in the CSV calculation.
        /// </summary>
        private const double CsvWeightMultiplier = 10;

        private static readonly Regex TravelNodeRegex = new Regex(@"\+# to (Strength|Intelligence|Dexterity)");

        /// <summary>
        /// Maps indexes of constraints (both attribute and pseudo attribute constraints to their {Target, Weight}-Tuple.
        /// </summary>
        private Tuple<float, double>[] _attrConstraints;
        /// <summary>
        /// Dictionary that maps attribute names to the constraint number they apply to (as indexes of _attrConstraints).
        /// </summary>
        private Dictionary<string, List<int>> _attrNameLookup;
        private Dictionary<Tuple<string, int>, float> _attrConversionMultipliers;

        private Dictionary<int, List<Tuple<int, float>>> _nodeAttributes;
        private Dictionary<int, bool> _areTravelNodes;

        private float[] _fixedAttributes;

        private HashSet<ushort> _fixedNodes;

        protected override GeneticAlgorithmParameters GaParameters
        {
            get
            {
                return new GeneticAlgorithmParameters(
                    (int)(GenMultiplier * (SearchSpace.Count < ConstRuntimeEndpoint ? (ConstRuntimeEndpoint * ConstRuntimeEndpoint) / SearchSpace.Count : SearchSpace.Count)),
                    (int)(PopMultiplier * SearchSpace.Count),
                    SearchSpace.Count, 6, 1);
            }
        }

        public AdvancedSolver(SkillTree tree, AdvancedSolverSettings settings)
            : base(tree, settings)
        { }

        protected override void BuildSearchGraph()
        {
            // Add start and check-tagged nodes as in SteinerSolver.
            SearchGraph = new SearchGraph();
            CreateStartNodes();
            CreateTargetNodes();
            // Set start and target nodes as the fixed nodes.
            _fixedNodes = new HashSet<ushort>(StartNodes.nodes.Select(node => node.Id));
            _fixedNodes.UnionWith(TargetNodes.Select(node => node.Id));
            
            var convertedPseudos = EvalPseudoAttrConstraints();

            // Assign a number to each attribute and pseudo attribute constraint
            // and link their names to these numbers.
            FormalizeConstraints(Settings.AttributeConstraints, convertedPseudos);

            // Extract attributes from nodes and set travel nodes.
            ExtractNodeAttributes();

            // Set fixed attributes from fixed nodes and Settings.InitialAttributes
            CreateFixedAttributes();

            CreateSearchGraph();
        }

        private void FormalizeConstraints(Dictionary<string, Tuple<float, double>> attrConstraints, List<ConvertedPseudoAttribute> pseudoConstraints)
        {
            _attrConstraints = new Tuple<float, double>[attrConstraints.Count + pseudoConstraints.Count];
            _attrNameLookup = new Dictionary<string, List<int>>(attrConstraints.Count);
            _attrConversionMultipliers = new Dictionary<Tuple<string, int>, float>(attrConstraints.Count);
            _fixedAttributes = new float[attrConstraints.Count + pseudoConstraints.Count];
            var i = 0;
            foreach (var kvPair in attrConstraints)
            {
                _attrConstraints[i] = kvPair.Value;
                _attrNameLookup[kvPair.Key] = new List<int> { i };
                _attrConversionMultipliers[Tuple.Create(kvPair.Key, i)] = 1;
                i++;
            }
            foreach (var pseudo in pseudoConstraints)
            {
                _attrConstraints[i] = pseudo.TargetWeightTuple;
                foreach (var tuple in pseudo.Attributes)
                {
                    if (_attrNameLookup.ContainsKey(tuple.Item1))
                    {
                        _attrNameLookup[tuple.Item1].Add(i);
                    }
                    else
                    {
                        _attrNameLookup[tuple.Item1] = new List<int> { i };
                    }
                    _attrConversionMultipliers[Tuple.Create(tuple.Item1, i)] = tuple.Item2;
                }
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
                var name = initialStat.Key;
                if (_attrNameLookup.ContainsKey(name))
                {
                    foreach (var i in _attrNameLookup[name])
                    {
                        var value = initialStat.Value * _attrConversionMultipliers[Tuple.Create(name, i)];
                        AddAttribute(Tuple.Create(i, value), _fixedAttributes);
                    }
                }
            }
        }

        private void ExtractNodeAttributes()
        {
            var skillNodes = Settings.SubsetTree.Count > 0
                ? Settings.SubsetTree.ToDictionary(id => id, id => SkillTree.Skillnodes[id])
                : SkillTree.Skillnodes;
            _nodeAttributes = new Dictionary<int, List<Tuple<int, float>>>(skillNodes.Count);
            _areTravelNodes = new Dictionary<int, bool>(skillNodes.Count);
            foreach (var node in skillNodes)
            {
                var id = node.Key;
                var skillNode = node.Value;

                // Remove attributes that have no constraints.
                // Replace attributes that have constraints with a tuple of their number and the value.
                // For attributes with more than one value, the first one is selected,
                // that is reasonable for the attributes the skill tree currently has.
                // Attributes without value are not supported, if a constraint without value slips
                // through, it will break.
                _nodeAttributes[id] =
                    (from attr in SkillTree.ExpandHybridAttributes(skillNode.Attributes)
                     where _attrNameLookup.ContainsKey(attr.Key)
                     from constraint in _attrNameLookup[attr.Key]
                     let value = attr.Value[0] * _attrConversionMultipliers[Tuple.Create(attr.Key, constraint)]
                     select Tuple.Create(constraint, value))
                    .ToList();

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
        }

        private List<ConvertedPseudoAttribute> EvalPseudoAttrConstraints()
        {
            var keystones = from nodeId in _fixedNodes
                            where SkillTree.Skillnodes[nodeId].IsKeyStone
                            select SkillTree.Skillnodes[nodeId].Name;
            var conditionSettings = new ConditionSettings(Settings.Tags, Settings.OffHand, keystones.ToArray(), Settings.WeaponClass);

            var resolvedWildcardNames = new Dictionary<string, List<Tuple<string, string[]>>>();
            var convertedPseudos = new List<ConvertedPseudoAttribute>(Settings.PseudoAttributeConstraints.Count);
            
            foreach (var pair in Settings.PseudoAttributeConstraints)
            {
                var convAttrs = new List<Tuple<string, float>>(pair.Key.Attributes.Count);
                foreach (var attr in pair.Key.Attributes)
                {
                    var name = attr.Name;
                    if (ContainsWildcardRegex.IsMatch(name))
                    {
                        if (!resolvedWildcardNames.ContainsKey(name))
                        {
                            var searchRegex = new Regex("^" + ContainsWildcardRegex.Replace(name, "(.*)") + "$");
                            resolvedWildcardNames[name] = (from a in SkillTree.AllAttributes
                                                           let match = searchRegex.Match(a)
                                                           where match.Success
                                                           select Tuple.Create(a, ExtractGroupValuesFromGroupCollection(match.Groups))).ToList();
                        }
                        convAttrs.AddRange(from replacement in resolvedWildcardNames[name]
                                           where attr.Eval(conditionSettings, replacement.Item2)
                                           select Tuple.Create(replacement.Item1, attr.ConversionMultiplier));
                    }
                    else if (attr.Eval(conditionSettings))
                    {
                        convAttrs.Add(Tuple.Create(name, attr.ConversionMultiplier));
                    }
                }

                var convPseudo = new ConvertedPseudoAttribute(convAttrs, pair.Value);
                convertedPseudos.Add(convPseudo);
            }

            return convertedPseudos;
        }

        private static string[] ExtractGroupValuesFromGroupCollection(GroupCollection groups)
        {
            var result = new string[groups.Count - 1];
            for (var i = 1; i < groups.Count; i++)
            {
                result[i] = groups[i].Value;
            }
            return result;
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
            var totalPoints = Settings.TotalPoints;
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
            if (usedNodeCount > totalPoints)
            {
                // If UsedNodeCount is higher than Settings.TotalPoints, it is 
                // calculated as a csv with a weight of 5. (and lower = better)
                csvs *= CalcCsv(2 * totalPoints - usedNodeCount, UsedNodeCountWeight, totalPoints);
            }
            else if (usedNodeCount < totalPoints)
            {
                // TODO test this factor, might not be optimal
                // optimal: least points spent with csv product = 1 (all constraints satisfied)
                // That means: minimize UsedNodeCount until Settings.TotalPoints > maximize csvs > minimize UsedNodeCount

                // If it is lower, apply it as a logarithmic factor.
                csvs *= 1 + UsedNodeCountFactor * Math.Log(totalPoints + 1 - usedNodeCount);
            }

            return csvs;
        }

        private static double CalcCsv(float x, double weight, float target)
        {
            // Don't go higher than the target value.
            // TODO how to handle csvs exceeding the target value? (curently they are capped at the target value)
            x = Math.Min(x, target);
            return Math.Exp(weight * CsvWeightMultiplier * x/target) / Math.Exp(weight * CsvWeightMultiplier);
        }
    }
}