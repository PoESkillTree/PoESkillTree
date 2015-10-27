using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    /// <summary>
    ///  A class controlling the interaction between the skill tree data and the
    ///  genetic algorithm used to find (hopefully) optimal solutions.
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
            get { return IsInitialized && CurrentStep >= MaxSteps; }
        }

        public abstract int MaxSteps { get; }

        public abstract int CurrentStep { get; }
        
        public IEnumerable<ushort> BestSolution { get; protected set; }

        private readonly SkillTree _tree;

        /// <summary>
        /// The SolverSettings that customize this solver run.
        /// </summary>
        protected readonly TS Settings;

        /// <summary>
        /// The fixed start node (can contain multiple) of this solver run.
        /// </summary>
        protected GraphNode StartNode { get; private set; }
        
        /// <summary>
        /// Gets the target nodes this solver run must include.
        /// </summary>
        protected IReadOnlyList<GraphNode> TargetNodes { get; private set; }

        /// <summary>
        /// Node enumeration in which all nodes that can be skilled lie.
        /// Simplification of the skill tree
        /// </summary>
        protected IReadOnlyList<GraphNode> AllNodes { get; private set; }
        
        /// <summary>
        /// Gets the list of GraphNodes from which this solver tries
        /// to find the best subset.
        /// </summary>
        protected IReadOnlyList<GraphNode> SearchSpace { get; private set; }

        /// <summary>
        /// DistanceLookup for calculating and caching distances and shortest paths between nodes.
        /// </summary>
        protected IDistanceLookup Distances { get; private set; }

        protected IReadOnlyDictionary<ushort, IReadOnlyCollection<ushort>> NodeExpansionDictionary { get; private set; }

        protected IReadOnlyGraphEdgeSet SearchSpaceEdgeSet { get; private set; }

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
        /// <exception cref="InvalidOperationException">
        /// If not all target nodes are connected to the start node.
        /// </exception>
        public virtual void Initialize()
        {
            BuildSearchGraph();

            SearchSpace = AllNodes.ToList();
            var variableTargetNodes = SearchSpace.Where(IsVariableTargetNode);
            var preProc = new SteinerPreprocessor(SearchSpace, TargetNodes, StartNode, variableTargetNodes);
            var remainingNodes = preProc.ReduceSearchSpace();

            BestSolution = preProc.LeastSolution;
            SearchSpace = remainingNodes.Except(TargetNodes).ToList();
            TargetNodes = preProc.FixedTargetNodes;
            Distances = preProc.DistanceLookup;
            SearchSpaceEdgeSet = preProc.EdgeSet;

            var expansionDict = remainingNodes.ToDictionary(n => n.Id, n => n.Nodes);
            foreach (var node in AllNodes)
            {
                if (!expansionDict.ContainsKey(node.Id))
                {
                    expansionDict.Add(node.Id, new[] {node.Id});
                }
            }
            NodeExpansionDictionary = expansionDict;

            Debug.WriteLine("Search space dimension: " + SearchSpace.Count);
            Debug.WriteLine("Target node count: " + TargetNodes.Count);

            IsInitialized = true;
        }

        public abstract void Step();

        public abstract void FinalStep();

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
            var searchGraph = new SearchGraph();
            CreateStartNodes(searchGraph);
            CreateTargetNodes(searchGraph);
            CreateSearchGraph(searchGraph);
            AllNodes = searchGraph.NodeDict.Values.ToList();
        }

        /// <summary>
        /// Initializes <see cref="StartNode"/>.
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
        /// Initializes <see cref="TargetNodes"/>.
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
        /// of the skill tree and including the that could be part of the solution.
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
                            || node.IsMastery)
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