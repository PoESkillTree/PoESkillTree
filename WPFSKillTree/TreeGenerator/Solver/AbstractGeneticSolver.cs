using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Algorithm;
using PoESkillTree.TreeGenerator.Algorithm.Model;
using PoESkillTree.TreeGenerator.Genetic;
using PoESkillTree.TreeGenerator.Settings;

namespace PoESkillTree.TreeGenerator.Solver
{
    /// <summary>
    /// Abstract solver that uses <see cref="GeneticAlgorithm"/> for solving.
    /// Subclasses at least need to provide a fitness function and <see cref="GeneticAlgorithmParameters"/>.
    /// </summary>
    /// <typeparam name="TS">The type of SolverSettings this solver uses.</typeparam>
    public abstract class AbstractGeneticSolver<TS> : AbstractSolver<TS>
        where TS : SolverSettings
    {
        
        public override int Steps
        {
            get { return IsInitialized ? Generations : 0; }
        }

        public override int CurrentStep
        {
            get { return IsInitialized ? _ga.GenerationCount : 0; }
        }

        public override int CurrentIteration
        {
            get { return IsInitialized ? _ga.CurrentIteration : 0; }
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
        /// Gets the number of generations of the genetic algorithm that should be calculated
        /// per iteration. One generation is calculated per step.
        /// </summary>
        protected abstract int Generations { get; }

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
                            prioQueue.Enqueue(new DirectedGraphEdge(i, j), Distances[i, j]);
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
            _ga.InitializeEvolution(GaParameters, new BitArray(SearchSpace.Count));
            _bestDna = _ga.GetBestDNA();
            _bestSolution = Extend(DnaToUsedNodes(_bestDna));
        }

        /// <summary>
        ///  Tells the genetic algorithm to advance one generation and processes
        ///  the resulting (possibly) improved solution.
        /// </summary>
        public override void Step()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Solver not initialized!");

            if (_ga.GenerationCount >= Steps)
            {
                _ga.NextIteration();
            }
            _ga.NewGeneration();

            if (_bestDna == null || !_ga.GetBestDNA().Equals(_bestDna))
            {
                _bestDna = _ga.GetBestDNA();
                _bestSolution = Extend(DnaToUsedNodes(_bestDna));
            }
        }

        private HashSet<ushort> Extend(IEnumerable<ushort> nodes)
        {
            return new HashSet<ushort>(nodes.SelectMany(n => NodeExpansionDictionary[n]));
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
                mst.Span(StartNode.DistancesIndex);

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
                _bestSolution = Extend(hillClimber.Improve(DnaToUsedNodes(_bestDna)));
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
