using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public abstract class AbstractGeneticSolver<TS> : AbstractSolver<TS> where TS : SolverSettings
    {

        public override int MaxSteps
        {
            get { return IsInitialized ? _ga.MaxGeneration : 0; }
        }

        public override int CurrentStep
        {
            get { return IsInitialized ? _ga.GenerationCount : 0; }
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
        /// Minimum ratio of number of target nodes to search space + target nodes
        /// to use a pre filled edge queue (that contains all possible edges) for mst calculation.
        /// </summary>
        private const double PreFilledSpanThreshold = 1.0 / 10;

        /// <summary>
        /// First edge of the linked list of edges ordered by priority.
        /// </summary>
        private LinkedGraphEdge _firstEdge;

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
            if (TargetNodes.Count / (double)totalCount >= PreFilledSpanThreshold)
            {
                var prioQueue = new LinkedListPriorityQueue<LinkedGraphEdge>(100);
                for (var i = 0; i < totalCount; i++)
                {
                    for (var j = i + 1; j < totalCount; j++)
                    {
                        prioQueue.Enqueue(new LinkedGraphEdge(i, j), Distances[i, j]);
                    }
                }
                _firstEdge = prioQueue.First;
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
                var skilledNodes = DnaToSpannedMst(_bestDna).GetUsedNodes();
                BestSolution = new HashSet<ushort>(skilledNodes.SelectMany(n => NodeExpansionDictionary[n]));
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
                mst.Span(StartNode);
            return mst;
        }

        public override void FinalStep()
        {
            if (FinalHillClimbEnabled)
            {
                var hillClimber = new HillClimber(FitnessFunction, TargetNodes, AllNodes);
                BestSolution = hillClimber.Improve(BestSolution);
            }
        }

        /// <summary>
        /// Calculates the fitness given a set of nodes that form the skill tree.
        /// </summary>
        /// <returns>a value indicating the fitness of the given nodes. Higher means better.</returns>
        protected abstract double FitnessFunction(HashSet<ushort> skilledNodes);

        private double FitnessFunction(BitArray dna)
        {
            using (var mst = DnaToSpannedMst(dna))
            {
                return FitnessFunction(mst.GetUsedNodes());
            }
        }
    }
}
