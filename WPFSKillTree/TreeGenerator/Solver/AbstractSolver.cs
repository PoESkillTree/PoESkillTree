using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Algorithm.Model;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
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
        
        public bool IsConsideredDone
        {
            get { return IsInitialized && CurrentIteration >= (Iterations - 1) && CurrentStep >= Steps; }
        }

        public abstract int Steps { get; }

        public abstract int CurrentStep { get; }

        public int Iterations { get { return Settings.Iterations; } }

        public abstract int CurrentIteration { get; }
        
        public abstract IEnumerable<ushort> BestSolution { get; }

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

        /// <summary>
        /// DistanceLookup for calculating and caching distances and shortest paths between nodes.
        /// </summary>
        protected IDistancePathLookup Distances { get; private set; }

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
        protected AbstractSolver(SkillTree tree, TS settings)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (settings == null) throw new ArgumentNullException("settings");

            IsInitialized = false;
            _tree = tree;
            Settings = settings;
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
            var remainingNodes = preProc.ReduceSearchSpace();

            TargetNodes = preProc.FixedTargetNodes;
            SearchSpace = remainingNodes.Except(TargetNodes).ToList();
            Distances = preProc.DistanceLookup;
            //SearchSpaceEdgeSet = preProc.EdgeSet;
            StartNode = preProc.StartNode;

            // SkillNode-Ids of the remaining search space may represent more than one node. This
            // information needs to be safed.
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
            UncountedNodes = 1 + StartNode.Nodes.Count(n => SkillTree.Skillnodes[n].ascendancyName != null);

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
            // This is really dirty but the whole code is so dependant on the skill tree stuff that
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
            if (Settings.SubsetTree.Count > 0 || Settings.InitialTree.Count > 0)
            {
                // if the current tree does not need to be part of the result, only skill the character node
                StartNode = searchGraph.SetStartNodes(new HashSet<ushort> { _tree.GetCharNodeId() });
            }
            else
            {
                StartNode = searchGraph.SetStartNodes(_tree.SkilledNodes);
            }
        }

        /// <summary>
        /// Initializes <see cref="TargetNodes"/> with all check-tagged nodes and the start node.
        /// Adds the check-tagged nodes to the provided SearchGraph.
        /// </summary>
        private void CreateTargetNodes(SearchGraph searchGraph)
        {
            TargetNodes = (from nodeId in Settings.Checked
                           where !searchGraph.NodeDict.ContainsKey(SkillTree.Skillnodes[nodeId])
                           select searchGraph.AddNodeId(nodeId))
                          .Union(new[] {StartNode}).ToList();
        }

        /// <summary>
        /// Initializes the search graph by going through all node groups
        /// of the skill tree and including those that could be part of the solution.
        /// </summary>
        private void CreateSearchGraph(SearchGraph searchGraph)
        {
            foreach (var ng in SkillTree.NodeGroups)
            {
                var mustInclude = false;

                SkillNode firstNeighbor = null;

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
                    foreach (var neighbor in node.Neighbor.Where(neighbor => neighbor.SkillNodeGroup != ng1))
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
                        if (SkillTree.rootNodeList.Contains(node.Id)
                            // Don't add nodes that are already in the graph (as
                            // target or start nodes).
                            || searchGraph.NodeDict.ContainsKey(node)
                            // Don't add nodes that should not be skilled.
                            || Settings.Crossed.Contains(node.Id)
                            // Only add nodes in the subsettree if one is given.
                            || Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(node.Id)
                            // Mastery nodes are obviously not useful.
                            || node.IsMastery
                            // Ignore ascendancies for now
                            || node.ascendancyName != null)
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