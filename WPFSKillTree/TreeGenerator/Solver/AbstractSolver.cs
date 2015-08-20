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

        public bool IsConsideredDone => IsInitialized && CurrentGeneration >= MaxGeneration;

        public int MaxGeneration => IsInitialized ? GaParameters.MaxGeneration : 0;

        public int CurrentGeneration => IsInitialized ? _ga.GenerationCount : 0;

        private BitArray _bestDna;

        public HashSet<ushort> BestSolution { get; private set; }

        protected MinimalSpanningTree LeastSolution { get; private set; }

        // TODO include alternative solutions
        //public IEnumerable<HashSet<ushort>> AlternativeSolutions { get; private set; }

        public SkillTree Tree { get; }

        protected readonly TS Settings;

        protected abstract GeneticAlgorithmParameters GaParameters { get; }

        protected Supernode StartNodes;

        protected HashSet<GraphNode> TargetNodes;

        protected SearchGraph SearchGraph;

        protected List<GraphNode> SearchSpace { get; private set; }

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

            SearchSpace = SearchGraph.nodeDict.Values.Where(IncludeNode).ToList();

            var consideredNodes = SearchSpace.Concat(TargetNodes).ToList();
            consideredNodes.Add(StartNodes);
            foreach (var node in consideredNodes)
            {
                node.Marked = true;
            }
            Distances.CalculateFully(consideredNodes);

            try
            {
                // Saving the leastSolution as initial solution. Makes sure there is always a
                // solution even if the search space is empty or MaxGeneration is 0.
                LeastSolution = CreateLeastSolution();
                BestSolution = SpannedMstToSkillnodes(LeastSolution);
                SearchSpace = SearchSpace.Where(IncludeNodeUsingDistances).ToList();
            }
            catch (KeyNotFoundException e)
            {
                throw new InvalidOperationException("The graph is disconnected.", e);
            }

            InitializeGa();

            IsInitialized = true;
        }

        // Needs to set SearchGraph, StartNodes and TargetNodes.
        protected abstract void BuildSearchGraph();

        // Whether the node should be included in the initial SearchSpace
        protected abstract bool IncludeNode(GraphNode node);

        protected abstract MinimalSpanningTree CreateLeastSolution();

        // Filtering that needs the distances calculated, called after CreateLeastSolution.
        protected abstract bool IncludeNodeUsingDistances(GraphNode node);

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
                if (nodes.Contains(SearchSpace[i].Id))
                {
                    dna[i] = true;
                }
            }

            return dna;
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

            var newSkilledNodes = new HashSet<ushort>(mst.UsedNodes);
            newSkilledNodes.UnionWith(StartNodes.nodes.Select(node => node.Id));
            return newSkilledNodes;
        }

        private MinimalSpanningTree DnaToMst(BitArray dna)
        {
            var mstNodes = new HashSet<GraphNode>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    mstNodes.Add(SearchSpace[i]);
            }

            mstNodes.Add(StartNodes);
            mstNodes.UnionWith(TargetNodes);

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