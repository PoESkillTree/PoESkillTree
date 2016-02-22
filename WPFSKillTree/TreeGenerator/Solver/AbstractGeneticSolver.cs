using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Genetic;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Abstract solver that uses <see cref="GeneticAlgorithm"/> for solving.
    /// Subclasses at least need to provide a fitness function and <see cref="GeneticAlgorithmParameters"/>.
    /// </summary>
    /// <typeparam name="TS">The type of SolverSettings this solver uses.</typeparam>
    public abstract class AbstractGeneticSolver<TS> : AbstractSolver<TS>
        where TS : SolverSettings
    {

        public override int MaxSteps
        {
            get { return IsInitialized ? _ga.MaxGeneration : 0; }
        }

        public override int CurrentStep
        {
            get { return IsInitialized ? _ga.GenerationCount : 0; }
        }

        private HashSet<ushort> _bestSolution;

        public override IEnumerable<ushort> BestSolution
        {
            get { return _bestSolution; }
        }

        /// <summary>
        /// The best dna calculated by the GA up to this point.
        /// </summary>
        private BitArray _bestDna;
        
        /// <summary>
        /// Gets the GaParameters to initialize the genetic algorithm with.
        /// </summary>
        protected abstract GeneticAlgorithmParameters GaParameters { get; }

        /// <summary>
        /// The genetic algorithm used by the solver to generate solutions.
        /// </summary>
        private GeneticAlgorithm _ga;

        /// <summary>
        /// Minimum ratio of number of target nodes to (search space + target nodes)
        /// to use a pre ordered edges for mst calculation.
        /// </summary>
        private const double PreFilledSpanThreshold = 1.0 / 10;

        /// <summary>
        /// List of edges ordered by priority.
        /// (used for mst calculation if <see cref="PreFilledSpanThreshold"/> is satisfied)
        /// </summary>
        private List<DirectedGraphEdge> _orderedEdges;

        /// <summary>
        /// Unordered edges accessible by the two DistanceIndexes of the nodes.
        /// (used for mst calculation if <see cref="PreFilledSpanThreshold"/> is not satisfied)
        /// </summary>
        private ITwoDArray<DirectedGraphEdge> _edges;

        /// <summary>
        /// Gets or sets whether this solver should try to improve the solution with simple HillClimbing
        /// after the final step.
        /// </summary>
        protected bool FinalHillClimbEnabled { private get; set; }

        protected AbstractGeneticSolver(SkillTree tree, TS settings)
            : base(tree, settings)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            
            OnFinalSearchSpaceCreated();

            var totalCount = SearchSpace.Count + TargetNodes.Count;

            // Create edges
            var edges = new DirectedGraphEdge[totalCount, totalCount];
            for (int i = 0; i < totalCount; i++)
            {
                for (int j = 0; j < totalCount; j++)
                {
                    edges[i, j] = new DirectedGraphEdge(i, j, Distances[i, j]);
                }
            }
            _edges = new TwoDArray<DirectedGraphEdge>(edges);

            // Sort edges if PreFilledSpanThreshold is satisfied.
            if (TargetNodes.Count/(double) totalCount >= PreFilledSpanThreshold)
            {
                using (var prioQueue = new LinkedListPriorityQueue<DirectedGraphEdge>(100, totalCount * totalCount))
                {
                    // A PriorityQueue is used for sorting. Should be faster than sorting-methods that don't exploit 
                    // sorting ints.
                    var enqueued = 0;
                    for (var i = 0; i < totalCount; i++)
                    {
                        for (var j = i + 1; j < totalCount; j++)
                        {
                            prioQueue.Enqueue(_edges[i, j]);
                            enqueued++;
                        }
                    }

                    _orderedEdges = new List<DirectedGraphEdge>(enqueued);
                    while (!prioQueue.IsEmpty)
                    {
                        _orderedEdges.Add(prioQueue.Dequeue());
                    }
                }
            }

            InitializeGa();
        }

        /// <summary>
        /// Called after search space and target nodes are in their final form and Initialization is finished except
        /// for the ga.
        /// Override to execute additional logic that needs the final search space.
        /// </summary>
        protected virtual void OnFinalSearchSpaceCreated()
        { }

        /// <summary>
        ///  Sets up the genetic algorithm to be ready for the evolutionary search.
        /// </summary>
        private void InitializeGa()
        {
            _ga = new GeneticAlgorithm(FitnessFunction);
            _ga.InitializeEvolution(GaParameters, TreeToDna(Settings.InitialTree));
            _bestDna = _ga.GetBestDNA();
            _bestSolution = DnaToSolution(_bestDna);
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
        public override void Step()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Solver not initialized!");

            _ga.NewGeneration();

            if ((_bestDna == null) || (GeneticAlgorithm.SetBits(_ga.GetBestDNA().Xor(_bestDna)) != 0))
            {
                _bestDna = _ga.GetBestDNA();
                _bestSolution = DnaToSolution(_bestDna);
            }
        }

        private HashSet<ushort> DnaToSolution(BitArray dna)
        {
            var skilledNodes = DnaToUsedNodes(dna);
            return new HashSet<ushort>(skilledNodes.SelectMany(n => NodeExpansionDictionary[n]));
        }

        private HashSet<ushort> DnaToUsedNodes(BitArray dna)
        {
            // Convert dna to corresponding GraphNodes.
            var mstNodes = new List<GraphNode>();
            var mstIndices = new List<int>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                {
                    mstNodes.Add(SearchSpace[i]);
                    mstIndices.Add(i);
                }
            }
            mstNodes.AddRange(TargetNodes);
            mstIndices.AddRange(TargetNodes.Select(n => n.DistancesIndex));

            // Calculate MST from nodes.
            var mst = new MinimalSpanningTree(mstIndices, Distances);
            if (_orderedEdges != null)
                mst.Span(_orderedEdges);
            else
                mst.Span(StartNode.DistancesIndex, _edges);

            // Convert GraphNodes and GraphEdges to SkillNode-Ids.
            var usedNodes = new HashSet<ushort>(mstNodes.Select(n => n.Id));
            foreach (var edge in mst.SpanningEdges)
            {
                usedNodes.UnionWith(Distances.GetShortestPath(edge.Inside, edge.Outside));
            }
            return usedNodes;
        }

        public override void FinalStep()
        {
            if (FinalHillClimbEnabled)
            {
                var hillClimber = new HillClimber(FitnessFunction, TargetNodes, AllNodes);
                _bestSolution = hillClimber.Improve(BestSolution);
            }
        }

        /// <summary>
        /// Calculates the fitness given a set of nodes that form the skill tree.
        /// </summary>
        /// <returns>a value indicating the fitness of the given nodes. Higher means better.</returns>
        protected abstract double FitnessFunction(HashSet<ushort> skilledNodes);

        private double FitnessFunction(BitArray dna)
        {
            return FitnessFunction(DnaToUsedNodes(dna));
        }
    }
}
