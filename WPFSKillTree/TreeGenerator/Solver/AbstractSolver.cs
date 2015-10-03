using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Interface of solver classes for use without generic parameter.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Gets whether the maximum number of steps is executed.
        /// </summary>
        bool IsConsideredDone { get; }

        /// <summary>
        /// Gets the maximum number of steps the solver executes.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        int MaxSteps { get; }

        /// <summary>
        /// Gets the number of steps executed up to this point.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        int CurrentStep { get; }

        /// <summary>
        /// Gets the best solution generated up to this point as
        /// HashSet of <see cref="SkillNode"/> ids.
        /// Return value is undefined until <see cref="Initialize"/> got called.
        /// </summary>
        IEnumerable<ushort> BestSolution { get; }

        /// <summary>
        /// Initializes the solver. Must be called before <see cref="Step"/>.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Progresses execution of the solver by one step.
        /// Call this function in a loop until <see cref="IsConsideredDone"/> is true.
        /// <see cref="Initialize"/> must be called before this.
        /// </summary>
        void Step();

        /// <summary>
        /// Optionally applies final algorithms after the last step to improve the solution.
        /// <see cref="Initialize"/> and <see cref="Step"/> must be called before this.
        /// </summary>
        void FinalStep();
    }

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
        private bool _isInitialized;
        
        public bool IsConsideredDone
        {
            get { return _isInitialized && CurrentStep >= MaxSteps; }
        }
        
        public int MaxSteps
        {
            get { return _isInitialized ? _ga.MaxGeneration : 0; }
        }
        
        public int CurrentStep
        {
            get { return _isInitialized ? _ga.GenerationCount : 0; }
        }
        
        /// <summary>
        /// The best dna calculated by the GA up to this point.
        /// </summary>
        private BitArray _bestDna;
        
        public IEnumerable<ushort> BestSolution { get; private set; }

        //public IEnumerable<HashSet<ushort>> AlternativeSolutions { get; private set; }

        private readonly SkillTree _tree;

        /// <summary>
        /// The SolverSettings that customize this solver run.
        /// </summary>
        protected readonly TS Settings;

        /// <summary>
        /// Gets the GaParameters to initialize the genetic algorithm with.
        /// </summary>
        protected abstract GeneticAlgorithmParameters GaParameters { get; }

        /// <summary>
        /// Gets or sets the fixed start nodes of this solver run.
        /// </summary>
        protected Supernode StartNodes { get; private set; }

        private HashSet<GraphNode> _targetNodes;
        /// <summary>
        /// Gets  the target nodes this solver run must include.
        /// </summary>
        protected IEnumerable<GraphNode> TargetNodes
        {
            get { return _targetNodes; }
        }

        /// <summary>
        /// The search graph in which all used nodes lie. Simplification
        /// of the skill tree.
        /// </summary>
        private SearchGraph _searchGraph;

        private List<GraphNode> _searchSpace;
        /// <summary>
        /// Gets the list of GraphNodes from which this solver tries
        /// to find the best subset.
        /// </summary>
        protected IReadOnlyCollection<GraphNode> SearchSpace
        {
            get { return _searchSpace; }
        }

        private HashSet<ushort> _fixedNodes;
        /// <summary>
        /// Get the collection of nodes always included in solutions.
        /// </summary>
        protected IEnumerable<ushort> FixedNodes
        {
            get { return _fixedNodes; }
        }

        /// <summary>
        /// The genetic algorithm used by the solver to generate solutions.
        /// </summary>
        private GeneticAlgorithm _ga;

        /// <summary>
        /// DistanceLookup for calculating and caching distances and shortest paths between nodes.
        /// </summary>
        protected readonly DistanceLookup Distances = new DistanceLookup();

        /// <summary>
        /// Gets or sets whether this solver should try to improve the solution with simple HillClimbing
        /// after the final step.
        /// </summary>
        protected bool FinalHillClimbEnabled { private get; set; }

        /// <summary>
        /// Creates a new, uninitialized instance.
        /// </summary>
        /// <param name="tree">The (not null) skill tree in which to optimize.</param>
        /// <param name="settings">The (not null) settings that describe what the solver should do.</param>
        protected AbstractSolver(SkillTree tree, TS settings)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (settings == null) throw new ArgumentNullException("settings");

            _isInitialized = false;
            _tree = tree;
            Settings = settings;
        }

        /// <summary>
        ///  Initializes the solver so that the optimization can be run.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If not all target nodes are connected to the start node.
        /// </exception>
        public void Initialize()
        {
            BuildSearchGraph();

            _searchSpace =
                _searchGraph.NodeDict.Values.Where(n => IncludeNode(n) && n != StartNodes && !TargetNodes.Contains(n))
                    .ToList();

            var consideredNodes = SearchSpace.Concat(TargetNodes).ToList();
            consideredNodes.Add(StartNodes);
            Distances.CalculateFully(consideredNodes);

            if (_targetNodes.Any(node => !Distances.AreConnected(StartNodes, node)))
            {
                throw new InvalidOperationException("The graph is disconnected.");
            }

            // Saving the leastSolution as initial solution. Makes sure there is always a
            // solution even if the search space is empty or MaxGeneration is 0.
            BestSolution = SpannedMstToSkillnodes(CreateLeastSolution());

            var removedNodes = new List<GraphNode>();
            var newSearchSpace = new List<GraphNode>();
            foreach (var node in SearchSpace)
            {
                if (IncludeNodeUsingDistances(node))
                {
                    newSearchSpace.Add(node);
                }
                else
                {
                    removedNodes.Add(node);
                }
            }
            _searchSpace = newSearchSpace;

            var remainingNodes = SearchSpace.Concat(TargetNodes).ToList();
            remainingNodes.Add(StartNodes);
            Distances.RemoveNodes(removedNodes, remainingNodes);

            InitializeGa();

            _isInitialized = true;
        }

        /// <summary>
        ///  Preprocesses the SkillTree graph into a simplified graph that omits
        ///  nodes (single pass) and contracts all skilled nodes into a single node.
        /// </summary>
        private void BuildSearchGraph()
        {
            _searchGraph = new SearchGraph();
            CreateStartNodes();
            CreateTargetNodes();
            // Set start and target nodes as the fixed nodes.
            _fixedNodes = new HashSet<ushort>(StartNodes.Nodes);
            _fixedNodes.UnionWith(_targetNodes.Select(node => node.Id));
            OnStartAndTargetNodesCreated();
            CreateSearchGraph();
        }

        /// <summary>
        /// Called after <see cref="StartNodes"/>, <see cref="TargetNodes"/> and
        /// <see cref="FixedNodes"/> are set.
        /// Override to execute additional logic that needs those calculated.
        /// </summary>
        protected virtual void OnStartAndTargetNodesCreated()
        { }

        /// <summary>
        /// Initializes <see cref="StartNodes"/>.
        /// </summary>
        private void CreateStartNodes()
        {
            if (Settings.SubsetTree.Count > 0 || Settings.InitialTree.Count > 0)
            {
                // if the current tree does not need to be part of the result, only skill the character node
                StartNodes = _searchGraph.SetStartNodes(new HashSet<ushort> { _tree.GetCharNodeId() });
            }
            else
            {
                StartNodes = _searchGraph.SetStartNodes(_tree.SkilledNodes);
            }
        }

        /// <summary>
        /// Initializes <see cref="TargetNodes"/>.
        /// </summary>
        private void CreateTargetNodes()
        {
            _targetNodes = new HashSet<GraphNode>();
            foreach (var nodeId in Settings.Checked)
            {
                // Don't add nodes that are already skilled.
                if (_searchGraph.NodeDict.ContainsKey(SkillTree.Skillnodes[nodeId]))
                    continue;
                // Don't add nodes that should not be skilled.
                if (Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(nodeId))
                    continue;
                // Add target node to the graph.
                var node = _searchGraph.AddNodeId(nodeId);
                _targetNodes.Add(node);
            }
        }

        /// <summary>
        /// Initializes <see cref="_searchGraph"/> by going through all node groups
        /// of the skill tree and including the that could be part of the solution.
        /// </summary>
        private void CreateSearchGraph()
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
                    if (_searchGraph.NodeDict.ContainsKey(node)
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
                            || _searchGraph.NodeDict.ContainsKey(node)
                            // Don't add nodes that should not be skilled.
                            || Settings.Crossed.Contains(node.Id)
                            // Only add nodes in the subsettree if one is given.
                            || Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(node.Id)
                            // Mastery nodes are obviously not useful.
                            || node.IsMastery)
                            continue;

                        if (IncludeNodeInSearchGraph(node))
                            _searchGraph.AddNode(node);
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

        /// <summary>
        /// Indicates whether the given node should be included initially in <see cref="SearchSpace"/>.
        /// </summary>
        /// <param name="node">The node in question (not null)</param>
        /// <returns>true if the given node should be included</returns>
        protected abstract bool IncludeNode(GraphNode node);

        /// <summary>
        /// Returns the least possible solution calculated by simply calculating the minimal spanning
        /// tree between start and target nodes.
        /// </summary>
        /// <returns></returns>
        private MinimalSpanningTree CreateLeastSolution()
        {
            // LeastSolution: MST between start and check-tagged nodes.
            var nodes = new List<GraphNode>(_targetNodes) { StartNodes };
            var leastSolution = new MinimalSpanningTree(nodes, Distances);
            leastSolution.Span(StartNodes);
            OnLeastSolutionCreated(leastSolution.SpanningEdges);
            return leastSolution;
        }

        /// <summary>
        /// Called after calculating the least possible solution which spans start and target nodes.
        /// Override to execute additional logic that needs those calculated.
        /// </summary>
        /// <param name="spanninEdges">The spanning edges of the least solution (not null)</param>
        protected virtual void OnLeastSolutionCreated(IEnumerable<GraphEdge> spanninEdges)
        { }

        /// <summary>
        /// Indicates whether the given node should be included in <see cref="SearchSpace"/> using
        /// the now calculated distances. Called after <see cref="IncludeNode"/> and after
        /// <see cref="OnLeastSolutionCreated"/>.
        /// </summary>
        /// <param name="node">The node in question (not null)</param>
        /// <returns></returns>
        protected abstract bool IncludeNodeUsingDistances(GraphNode node);

        /// <summary>
        ///  Sets up the genetic algorithm to be ready for the evolutionary search.
        /// </summary>
        private void InitializeGa()
        {
            Debug.WriteLine("Search space dimension: " + SearchSpace.Count);

            _ga = new GeneticAlgorithm(FitnessFunction);
            _ga.InitializeEvolution(GaParameters, TreeToDna(Settings.InitialTree));
        }

        private BitArray TreeToDna(HashSet<ushort> nodes)
        {
            var dna = new BitArray(SearchSpace.Count);
            for (var i = 0; i < SearchSpace.Count; i++)
            {
                if (nodes.Contains(_searchSpace[i].Id))
                {
                    dna[i] = true;
                }
            }
            return dna;
        }

        /// <summary>
        ///  Tells the genetic algorithm to advance one generation and processes
        ///  the resulting (possibly) improved solution.
        /// </summary>
        public void Step()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Solver not initialized!");

            _ga.NewGeneration();

            if ((_bestDna == null) || (GeneticAlgorithm.SetBits(_ga.GetBestDNA().Xor(_bestDna)) != 0))
            {
                _bestDna = _ga.GetBestDNA();
                var bestMst = DnaToMst(_bestDna);
                bestMst.Span(StartNodes);
                BestSolution = SpannedMstToSkillnodes(bestMst);
            }
        }

        public void FinalStep()
        {
            if (FinalHillClimbEnabled)
            {
                var hillClimber = new HillClimber(FitnessFunction, FixedNodes, _searchGraph.NodeDict.Values);
                BestSolution = hillClimber.Improve(BestSolution);
            }
        }

        /// <summary>
        ///  Converts an MST spanning a set of GraphNodes back into its equivalent
        ///  as a HashSet of SkillNode IDs.
        /// </summary>
        /// <param name="mst">The spanned MinimalSpanningTree.</param>
        /// <returns>A HashSet containing the node IDs of all SkillNodes spanned
        /// by the MST.</returns>
        private IEnumerable<ushort> SpannedMstToSkillnodes(MinimalSpanningTree mst)
        {
            if (!mst.IsSpanned)
                throw new Exception("The passed MST is not spanned!");

            var newSkilledNodes = mst.GetUsedNodes();
            newSkilledNodes.UnionWith(StartNodes.Nodes);
            return newSkilledNodes;
        }

        private MinimalSpanningTree DnaToMst(BitArray dna)
        {
            var mstNodes = new List<GraphNode>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    mstNodes.Add(_searchSpace[i]);
            }

            mstNodes.Add(StartNodes);
            mstNodes.AddRange(_targetNodes);

            return new MinimalSpanningTree(mstNodes, Distances);
        }

        private double FitnessFunction(BitArray dna)
        {
            var mst = DnaToMst(dna);
            mst.Span(StartNodes);
            return FitnessFunction(mst.GetUsedNodes());
        }

        /// <summary>
        /// Calculates the fitness given a set of nodes that form the skill tree.
        /// </summary>
        /// <returns>a value indicating the fitness of the given nodes. Higher means better.</returns>
        protected abstract double FitnessFunction(HashSet<ushort> skilledNodes);
    }
}