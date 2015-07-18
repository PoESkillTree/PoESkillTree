using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    // Non-generic interface for usage without specifying the generic parameter.
    public interface ISolver
    {
        bool IsConsideredDone { get; }

        int MaxGeneration { get; }

        int CurrentGeneration { get; }

        HashSet<ushort> BestSolution { get; }

        SkillTree Tree { get; }

        void Initialize();

        void Step();
    }

    public abstract class AbstractSolver<TS> : ISolver where TS : SolverSettings
    {
        public bool IsInitialized { get; private set; }

        public bool IsConsideredDone { get { return IsInitialized && CurrentGeneration >= MaxGeneration; } }

        public int MaxGeneration { get { return GaParameters.MaxGeneration; } }

        public int CurrentGeneration { get { return IsInitialized ? _ga.GenerationCount : 0; } }

        private BitArray _bestDna;

        public HashSet<ushort> BestSolution { get; private set; }

        // TODO include alternative solutions
        //public IEnumerable<HashSet<ushort>> AlternativeSolutions { get; private set; }

        public SkillTree Tree { get; private set; }

        protected readonly TS Settings;

        protected abstract GeneticAlgorithmParameters GaParameters { get; }

        protected Supernode StartNodes;

        protected HashSet<GraphNode> TargetNodes;

        protected SearchGraph SearchGraph;

        protected List<GraphNode> SearchSpace;

        private GeneticAlgorithm _ga;

        protected readonly DistanceLookup Distances = new DistanceLookup();

        protected AbstractSolver(SkillTree tree, TS settings)
        {
            IsInitialized = false;
            Tree = tree;
            Settings = settings;
        }

        public void Initialize()
        {
            BuildSearchGraph();

            try
            {
                var leastSolution = BuildSearchSpace();

                // Saving the leastSolution as initial solution. Makes sure there is always a
                // solution even if the search space is empty or MaxGeneration is 0.
                BestSolution = SpannedMstToSkillnodes(leastSolution);
            }
            catch (DistanceLookup.GraphNotConnectedException e)
            {
                throw new InvalidOperationException("The graph is disconnected.", e);
            }

            InitializeGa();

            IsInitialized = true;
        }

        protected abstract void BuildSearchGraph();

        protected abstract MinimalSpanningTree BuildSearchSpace();

        private void InitializeGa()
        {
            Debug.WriteLine("Search space dimension: " + SearchSpace.Count);

            _ga = new GeneticAlgorithm(FitnessFunction);
            _ga.InitializeEvolution(GaParameters);
        }

        public void Step()
        {
            if (!IsInitialized)
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

        private HashSet<ushort> SpannedMstToSkillnodes(MinimalSpanningTree mst)
        {
            if (!mst.IsSpanned)
                throw new Exception("The passed MST is not spanned!");

            var newSkilledNodes = new HashSet<ushort>();
            newSkilledNodes.UnionWith(StartNodes.nodes.Select(node => node.Id));
            foreach (var edge in mst.SpanningEdges)
            {
                newSkilledNodes.UnionWith(Distances.GetShortestPath(edge));
                newSkilledNodes.Add(edge.outside.Id);
                newSkilledNodes.Add(edge.inside.Id);
            }
            return newSkilledNodes;
        }

        private MinimalSpanningTree DnaToMst(BitArray dna)
        {
            var usedSteinerPoints = new List<GraphNode>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    usedSteinerPoints.Add(SearchSpace[i]);
            }

            var mstNodes = new HashSet<GraphNode>(usedSteinerPoints) {StartNodes};

            foreach (var targetNode in TargetNodes)
            {
                mstNodes.Add(targetNode);
            }

            return new MinimalSpanningTree(mstNodes, Distances);
        }

        private double FitnessFunction(BitArray dna)
        {
            var mst = DnaToMst(dna);
            mst.Span(StartNodes);

            return FitnessFunction(mst);
        }

        protected abstract double FitnessFunction(MinimalSpanningTree tree);
    }
}