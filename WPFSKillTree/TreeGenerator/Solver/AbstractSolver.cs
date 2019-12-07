using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Algorithm;
using PoESkillTree.TreeGenerator.Algorithm.Model;
using PoESkillTree.TreeGenerator.Settings;

namespace PoESkillTree.TreeGenerator.Solver
{
    /// <summary>
    ///  Base solver class controlling the interaction between the skill tree data and the
    ///  algorithm used to find (hopefully) optimal solutions.
    /// </summary>
    /// <typeparam name="TS">The type of SolverSettings this solver uses.</typeparam>
    public abstract class AbstractSolver<TS> : ISolver
        where TS : SolverSettings
    {
        /// <summary>
        /// True once <see cref="Initialize"/> returned.
        /// </summary>
        protected bool IsInitialized { get; private set; }
        
        public bool IsConsideredDone => IsInitialized && CurrentIteration >= (Iterations - 1) && CurrentStep >= Steps;

        public abstract int Steps { get; }

        public abstract int CurrentStep { get; }

        public int Iterations => Settings.Iterations;

        public abstract int CurrentIteration { get; }
        
        public abstract IEnumerable<ushort> BestSolution { get; protected set; }

        /// <summary>
        /// Skill tree instance to operate on.
        /// </summary>
        private readonly SkillTree _tree;

        /// <summary>
        /// The SolverSettings that customize this solver run.
        /// </summary>
        protected readonly TS Settings;
        
        public int UncountedNodes { get; private set; }

        /// <summary>
        /// The fixed start node (can contain multiple) of this solver run.
        /// </summary>
        protected GraphNode StartNode { get; private set; }
        
        /// <summary>
        /// Gets the target nodes this solver run must include.
        /// </summary>
        protected IReadOnlyList<GraphNode> TargetNodes { get; private set; }

        /// <summary>
        /// Contains all nodes that can be skilled. Simplification of the skill tree.
        /// </summary>
        protected IReadOnlyList<GraphNode> AllNodes { get; private set; }
        
        /// <summary>
        /// Gets the list of GraphNodes from which this solver tries to find the best subset.
        /// </summary>
        protected IReadOnlyList<GraphNode> SearchSpace { get; private set; }

        protected DistanceLookup Distances { get; private set; }

        protected ShortestPathLookup ShortestPaths { get; private set; }

        /// <summary>
        /// Nodes may be merged by the Preprocessor. This Pseudo-Dictionary contains the node-ids that are represented
        /// by a single node-id. If the node is unmerged, it only contains itself.
        /// Fits all possible ushorts (node ids) and is pretty sparse. Not contained ids have null as value.
        /// </summary>
        protected IReadOnlyList<IReadOnlyCollection<ushort>> NodeExpansionDictionary { get; private set; }

        // May become useful at some point if edge-based-optimization (instead of node-based) is implemented.
        //protected IReadOnlyGraphEdgeSet SearchSpaceEdgeSet { get; private set; }

        /// <summary>
        /// Creates a new, uninitialized instance.
        /// </summary>
        /// <param name="tree">The (not null) skill tree in which to optimize.</param>
        /// <param name="settings">The (not null) settings that describe what the solver should do.</param>
#pragma warning disable CS8618 // Initialized in Inittialize
        protected AbstractSolver(SkillTree tree, TS settings)
#pragma warning restore
        {
            IsInitialized = false;
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        ///  Initializes the solver so that the optimization can be run.
        /// </summary>
        /// <exception cref="GraphNotConnectedException">
        /// If not all target nodes are connected to the start node.
        /// </exception>
        public virtual void Initialize()
        {
            BuildSearchGraph();

            // Use SteinerPreprocessor for search space reduction.
            SearchSpace = AllNodes.ToList();
            var variableTargetNodes = SearchSpace.Where(IsVariableTargetNode);
            var preProc = new SteinerPreprocessor(SearchSpace, TargetNodes, StartNode, variableTargetNodes);
            var result = preProc.ReduceSearchSpace();

            TargetNodes = result.FixedTargetNodes;
            var remainingNodes = result.RemainingNodes;
            SearchSpace = remainingNodes.Except(TargetNodes).ToList();
            Distances = result.DistanceLookup;
            ShortestPaths = result.ShortestPathLookup;
            StartNode = result.StartNode;

            // SkillNode-Ids of the remaining search space may represent more than one node. This
            // information needs to be saved.
            var expansionDict = new IReadOnlyCollection<ushort>[ushort.MaxValue];
            foreach (var node in remainingNodes)
            {
                expansionDict[node.Id] = node.Nodes;
            }
            var inExpansion = new HashSet<ushort>(remainingNodes.SelectMany(n => n.Nodes));
            // Add the remaining nodes as single (unmerged) ones.
            foreach (var node in AllNodes)
            {
                if (!inExpansion.Contains(node.Id))
                {
                    expansionDict[node.Id] = new[] {node.Id};
                }
            }
            NodeExpansionDictionary = expansionDict;
            
            // The hidden root node and ascendancy nodes do not count for the total node count.
            UncountedNodes = 1 + StartNode.Nodes.Count(n => SkillTree.Skillnodes[n].IsAscendancyNode);

            Debug.WriteLine("Search space dimension: " + SearchSpace.Count);
            Debug.WriteLine("Target node count: " + TargetNodes.Count);

            IsInitialized = true;
        }

        public abstract void Step();

        public abstract void FinalStep();

        /// <summary>
        /// Returns true iff the given node can't be removed from the search space because it can become a target
        /// node in some instances (if optimizing for something else than simple Steiner).
        /// Overwrite if there are variable target nodes.
        /// </summary>
        protected virtual bool IsVariableTargetNode(GraphNode node)
        {
            return false;
        }

        /// <summary>
        ///  Preprocesses the SkillTree graph into a simplified graph that omits
        ///  nodes (single pass) and contracts all skilled nodes into a single node.
        /// </summary>
        private void BuildSearchGraph()
        {
            // Make all directed edges from Scion Ascendant "Path of the ..." node undirected.
            // This is really dirty but the whole code is so dependent on the skill tree stuff that
            // I don't see a non dirty way.
            var ascendantClassStartNodes = SkillTree.Skillnodes.Values.Where(SkillTree.IsAscendantClassStartNode).ToList();
            foreach (var classStartNode in ascendantClassStartNodes)
            {
                foreach (var neighbor in classStartNode.Neighbor)
                {
                    neighbor.Neighbor.Add(classStartNode);
                }
            }

            var searchGraph = new SearchGraph();
            CreateStartNodes(searchGraph);
            CreateTargetNodes(searchGraph);
            CreateSearchGraph(searchGraph);
            AllNodes = searchGraph.NodeDict.Values.ToList();

            // Remove all added edges again.
            foreach (var classStartNode in ascendantClassStartNodes)
            {
                foreach (var neighbor in classStartNode.Neighbor)
                {
                    neighbor.Neighbor.Remove(classStartNode);
                }
            }
        }

        /// <summary>
        /// Initializes <see cref="StartNode"/> and sets the start node in the provided SearchGraph.
        /// </summary>
        private void CreateStartNodes(SearchGraph searchGraph)
        {
            StartNode = searchGraph.SetStartNodes(_tree.SkilledNodes);
        }

        /// <summary>
        /// Initializes <see cref="TargetNodes"/> with all check-tagged nodes and the start node.
        /// Adds the check-tagged nodes to the provided SearchGraph.
        /// </summary>
        private void CreateTargetNodes(SearchGraph searchGraph)
        {
            TargetNodes = (from node in Settings.Checked
                           where !searchGraph.NodeDict.ContainsKey(node)
                           select searchGraph.AddNodeId(node.Id))
                          .Union(new[] {StartNode}).ToList();
        }

        /// <summary>
        /// Initializes the search graph by going through all node groups
        /// of the skill tree and including those that could be part of the solution.
        /// </summary>
        private void CreateSearchGraph(SearchGraph searchGraph)
        {
            foreach (var i in SkillTree.PoESkillTree.Groups)
            {
                var ng = i.Value;
                var mustInclude = false;

                SkillNode? firstNeighbor = null;

                // Find out if this node group can be omitted.
                foreach (var node in ng.Nodes)
                {
                    // If the group contains a skilled node or a target node,
                    // it can't be omitted.
                    if (searchGraph.NodeDict.ContainsKey(node)
                        || MustIncludeNodeGroup(node))
                    {
                        mustInclude = true;
                        break;
                    }

                    // If the group is adjacent to more than one node, it must
                    // also be fully included (since it's not isolated and could
                    // be part of a path to other nodes).
                    var ng1 = ng;
                    foreach (var neighbor in node.Neighbor.Where(neighbor => neighbor.Group != ng1))
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
                    foreach (var node in ng.Nodes)
                    {
                        // Can't path through class starts.
                        if (node.IsRootNode
                            // Don't add nodes that are already in the graph (as
                            // target or start nodes).
                            || searchGraph.NodeDict.ContainsKey(node)
                            // Don't add nodes that should not be skilled.
                            || Settings.Crossed.Contains(node)
                            // Mastery nodes are obviously not useful.
                            || node.Type == PassiveNodeType.Mastery
                            // Ignore ascendencies for now
                            || node.IsAscendancyNode)
                            continue;

                        if (IncludeNodeInSearchGraph(node))
                            searchGraph.AddNode(node);
                    }
                }
            }
        }

        /// <summary>
        /// Override to force including node groups into the search graph with additional conditions.
        /// </summary>
        /// <param name="node">The node in question (not null)</param>
        /// <returns>true if not overriden</returns>
        protected virtual bool MustIncludeNodeGroup(SkillNode node)
        {
            return false;
        }

        /// <summary>
        /// Override to exclude node from the search graph with additional conditions.
        /// </summary>
        /// <param name="node">The node in question (not null)</param>
        /// <returns>false if not overriden</returns>
        protected virtual bool IncludeNodeInSearchGraph(SkillNode node)
        {
            return true;
        }
    }
}