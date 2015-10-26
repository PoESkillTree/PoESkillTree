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
        /// The fixed start node (can contain multiple) of this solver run.
        /// </summary>
        private GraphNode _startNode;
        
        /// <summary>
        /// Gets the target nodes this solver run must include.
        /// </summary>
        public IReadOnlyList<GraphNode> TargetNodes { get; private set; }

        /// <summary>
        /// The search graph in which all used nodes lie. Simplification
        /// of the skill tree.
        /// </summary>
        private SearchGraph _searchGraph;
        
        /// <summary>
        /// Gets the list of GraphNodes from which this solver tries
        /// to find the best subset.
        /// </summary>
        public IReadOnlyList<GraphNode> SearchSpace { get; private set; }

        /// <summary>
        /// The genetic algorithm used by the solver to generate solutions.
        /// </summary>
        private GeneticAlgorithm _ga;

        /// <summary>
        /// DistanceLookup for calculating and caching distances and shortest paths between nodes.
        /// </summary>
        public IDistanceLookup Distances { get; private set; }

        /// <summary>
        /// Gets or sets whether this solver should try to improve the solution with simple HillClimbing
        /// after the final step.
        /// </summary>
        protected bool FinalHillClimbEnabled { private get; set; }

        /// <summary>
        /// Minimum ratio of number of target nodes to search space + target nodes
        /// to use a pre filled edge queue (that contains all possible edges) for mst calculation.
        /// </summary>
        private const double PreFilledSpanThreshold = 1.0 / 10;

        /// <summary>
        /// First edge of the linked list of edges ordered by priority.
        /// </summary>
        private LinkedGraphEdge _firstEdge;

        protected IReadOnlyDictionary<ushort, IReadOnlyCollection<ushort>> NodeExpansionDictionary { get; private set; }

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

            SearchSpace = _searchGraph.NodeDict.Values.ToList();
            var variableTargetNodes = SearchSpace.Where(IsVariableTargetNode);
            var preProc = new SteinerPreprocessor(SearchSpace, TargetNodes, _startNode, variableTargetNodes);
            var remainingNodes = preProc.ReduceSearchSpace();

            BestSolution = preProc.LeastSolution;
            SearchSpace = remainingNodes.Except(TargetNodes).ToList();
            TargetNodes = preProc.FixedTargetNodes;
            Distances = preProc.DistanceLookup;

            var expansionDict = remainingNodes.ToDictionary(n => n.Id, n => n.Nodes);
            foreach (var node in _searchGraph.NodeDict.Values)
            {
                if (!expansionDict.ContainsKey(node.Id))
                {
                    expansionDict.Add(node.Id, new[] {node.Id});
                }
            }
            NodeExpansionDictionary = expansionDict;

            if (TargetNodes.Count/(double)remainingNodes.Count >= PreFilledSpanThreshold)
            {
                var prioQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);
                for (var i = 0; i < remainingNodes.Count; i++)
                {
                    for (var j = i + 1; j < remainingNodes.Count; j++)
                    {
                        prioQueue.Enqueue(new LinkedGraphEdge(i, j), Distances[i, j]);
                    }
                }
                _firstEdge = prioQueue.First;
            }

            Debug.WriteLine("Search space dimension: " + SearchSpace.Count);
            Debug.WriteLine("Target node count: " + TargetNodes.Count);

            OnFinalSearchSpaceCreated();

            InitializeGa();

            _isInitialized = true;
        }

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
            _searchGraph = new SearchGraph();
            CreateStartNodes();
            CreateTargetNodes();
            OnTargetNodesCreated();
            CreateSearchGraph();
        }

        /// <summary>
        /// Called after <see cref="TargetNodes"/> are set.
        /// Override to execute additional logic that needs those calculated.
        /// </summary>
        protected virtual void OnTargetNodesCreated()
        { }

        /// <summary>
        /// Called after search space and target nodes are in their final form and Initialization is finished except
        /// for the ga.
        /// Override to execute additional logic that needs the final search space.
        /// </summary>
        protected virtual void OnFinalSearchSpaceCreated()
        { }

        /// <summary>
        /// Initializes <see cref="_startNode"/>.
        /// </summary>
        private void CreateStartNodes()
        {
            if (Settings.SubsetTree.Count > 0 || Settings.InitialTree.Count > 0)
            {
                // if the current tree does not need to be part of the result, only skill the character node
                _startNode = _searchGraph.SetStartNodes(new HashSet<ushort> { _tree.GetCharNodeId() });
            }
            else
            {
                _startNode = _searchGraph.SetStartNodes(_tree.SkilledNodes);
            }
        }

        /// <summary>
        /// Initializes <see cref="TargetNodes"/>.
        /// </summary>
        private void CreateTargetNodes()
        {
            TargetNodes = (from nodeId in Settings.Checked
                           where !_searchGraph.NodeDict.ContainsKey(SkillTree.Skillnodes[nodeId])
                           select _searchGraph.AddNodeId(nodeId))
                          .Union(new[] {_startNode}).ToList();
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
        ///  Sets up the genetic algorithm to be ready for the evolutionary search.
        /// </summary>
        private void InitializeGa()
        {
            _ga = new GeneticAlgorithm(FitnessFunction);
            _ga.InitializeEvolution(GaParameters, TreeToDna(Settings.InitialTree));
        }

        private BitArray TreeToDna(HashSet<ushort> nodes)
        {
            var dna = new BitArray(SearchSpace.Count);
            for (var i = 0; i < SearchSpace.Count; i++)
            {
                if (nodes.Contains(SearchSpace[i].Id))
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
                var skilledNodes = DnaToSpannedMst(_bestDna).GetUsedNodes();
                BestSolution = new HashSet<ushort>(skilledNodes.SelectMany(n => NodeExpansionDictionary[n]));
            }
        }

        public void FinalStep()
        {
            if (FinalHillClimbEnabled)
            {
                var hillClimber = new HillClimber(FitnessFunction, TargetNodes, _searchGraph.NodeDict.Values);
                BestSolution = hillClimber.Improve(BestSolution);
            }
        }
        
        private MinimalSpanningTree DnaToSpannedMst(BitArray dna)
        {
            var mstNodes = new List<GraphNode>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    mstNodes.Add(SearchSpace[i]);
            }
            mstNodes.AddRange(TargetNodes);

            var mst = new MinimalSpanningTree(mstNodes, Distances);
            if (_firstEdge != null)
                mst.Span(_firstEdge);
            else
                mst.Span(_startNode);
            return mst;
        }

        private double FitnessFunction(BitArray dna)
        {
            using (var mst = DnaToSpannedMst(dna))
            {
                return FitnessFunction(mst.GetUsedNodes());
            }
        }

        /// <summary>
        /// Calculates the fitness given a set of nodes that form the skill tree.
        /// </summary>
        /// <returns>a value indicating the fitness of the given nodes. Higher means better.</returns>
        protected abstract double FitnessFunction(HashSet<ushort> skilledNodes);
    }
}