﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public abstract class AbstractSolver<TS> where TS : SolverSettings
    {
        public bool IsInitialized { get; private set; }

        public bool IsConsideredDone { get { return IsInitialized && CurrentGeneration >= MaxGeneration; } }

        public int MaxGeneration { get { return GaParameters.MaxGeneration; } }

        public int CurrentGeneration { get { return IsInitialized ? _ga.GenerationCount : 0; } }

        private BitArray _bestDna;

        public HashSet<ushort> BestSolution { get; private set; }

        //public IEnumerable<HashSet<ushort>> AlternativeSolutions { get; private set; }

        public readonly SkillTree Tree;

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
            // Build a tree only containing the start nodes. Makes sure BestSolution
            // is not null or empty if MaxGeneration is 0.
            BestSolution = new HashSet<ushort>(StartNodes.nodes.Select(node => node.Id));

            try
            {
                BuildSearchSpace();
            }
            catch (DistanceLookup.GraphNotConnectedException e)
            {
                throw new InvalidOperationException("The graph is disconnected.", e);
            }

            InitializeGa();

            IsInitialized = true;
        }

        protected abstract void BuildSearchGraph();

        protected abstract void BuildSearchSpace();

        private void InitializeGa()
        {
#if DEBUG
            Console.WriteLine(@"Search space dimension: " + SearchSpace.Count);
#endif
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