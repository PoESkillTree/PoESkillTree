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

        void FinalStep();
    }

    public abstract class AbstractSolver<TS> : ISolver where TS : SolverSettings
    {
        private bool _isInitialized;

        public bool IsConsideredDone
        {
            get { return _isInitialized && CurrentGeneration >= MaxGeneration; }
        }

        public int MaxGeneration
        {
            get { return _isInitialized ? GaParameters.MaxGeneration : 0; }
        }

        public int CurrentGeneration
        {
            get { return _isInitialized ? _ga.GenerationCount : 0; }
        }

        private BitArray _bestDna;

        public HashSet<ushort> BestSolution { get; private set; }

        //public IEnumerable<HashSet<ushort>> AlternativeSolutions { get; private set; }

        public SkillTree Tree { get; private set; }

        protected readonly TS Settings;

        protected abstract GeneticAlgorithmParameters GaParameters { get; }

        protected Supernode StartNodes { get; private set; }

        protected HashSet<GraphNode> TargetNodes { get; private set; }

        protected SearchGraph SearchGraph { get; private set; }

        protected List<GraphNode> SearchSpace { get; private set; }

        protected HashSet<ushort> FixedNodes { get; private set; }

        private GeneticAlgorithm _ga;

        protected readonly DistanceLookup Distances = new DistanceLookup();

        protected virtual bool FinalHillClimbEnabled
        {
            get { return true; }
        }

        protected AbstractSolver(SkillTree tree, TS settings)
        {
            _isInitialized = false;
            Tree = tree;
            Settings = settings;
        }

        public void Initialize()
        {
            BuildSearchGraph();

            SearchSpace = SearchGraph.NodeDict.Values.Where(IncludeNode).ToList();

            var consideredNodes = SearchSpace.Concat(TargetNodes).ToList();
            consideredNodes.Add(StartNodes);
            Distances.CalculateFully(consideredNodes);

            try
            {
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
                SearchSpace = newSearchSpace;

                var remainingNodes = SearchSpace.Concat(TargetNodes).ToList();
                remainingNodes.Add(StartNodes);
                Distances.RemoveNodes(removedNodes, remainingNodes);
            }
            catch (GraphNotConnectedException e)
            {
                throw new InvalidOperationException("The graph is disconnected.", e);
            }

            InitializeGa();

            _isInitialized = true;
        }
        
        private void BuildSearchGraph()
        {
            SearchGraph = new SearchGraph();
            CreateStartNodes();
            CreateTargetNodes();
            // Set start and target nodes as the fixed nodes.
            FixedNodes = new HashSet<ushort>(StartNodes.Nodes.Select(node => node.Id));
            FixedNodes.UnionWith(TargetNodes.Select(node => node.Id));
            OnStartAndTargetNodesCreated();
            CreateSearchGraph();
        }

        protected virtual void OnStartAndTargetNodesCreated()
        {
        }

        private void CreateStartNodes()
        {
            if (Settings.SubsetTree.Count > 0 || Settings.InitialTree.Count > 0)
            {
                // if the current tree does not need to be part of the result, only skill the character node
                StartNodes = SearchGraph.SetStartNodes(new HashSet<ushort> { Tree.GetCharNodeId() });
            }
            else
            {
                StartNodes = SearchGraph.SetStartNodes(Tree.SkilledNodes);
            }
        }

        private void CreateTargetNodes()
        {
            TargetNodes = new HashSet<GraphNode>();
            foreach (var nodeId in Settings.Checked)
            {
                // Don't add nodes that are already skilled.
                if (SearchGraph.NodeDict.ContainsKey(SkillTree.Skillnodes[nodeId]))
                    continue;
                // Don't add nodes that should not be skilled.
                if (Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(nodeId))
                    continue;
                // Add target node to the graph.
                var node = SearchGraph.AddNodeId(nodeId);
                TargetNodes.Add(node);
            }
        }

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
                    if (SearchGraph.NodeDict.ContainsKey(node)
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
                            || SearchGraph.NodeDict.ContainsKey(node)
                            // Don't add nodes that should not be skilled.
                            || Settings.Crossed.Contains(node.Id)
                            // Only add nodes in the subsettree if one is given.
                            || Settings.SubsetTree.Count > 0 && !Settings.SubsetTree.Contains(node.Id)
                            // Mastery nodes are obviously not useful.
                            || node.IsMastery)
                            continue;

                        if (IncludeNodeInSearchGraph(node))
                            SearchGraph.AddNode(node);
                    }
                }
            }
        }

        protected virtual bool MustIncludeNodeGroup(SkillNode node)
        {
            return false;
        }

        protected virtual bool IncludeNodeInSearchGraph(SkillNode node)
        {
            return true;
        }

        // Whether the node should be included in the initial SearchSpace
        protected abstract bool IncludeNode(GraphNode node);

        private MinimalSpanningTree CreateLeastSolution()
        {
            // LeastSolution: MST between start and check-tagged nodes.
            var nodes = new List<GraphNode>(TargetNodes) { StartNodes };
            var leastSolution = new MinimalSpanningTree(nodes, Distances);
            leastSolution.Span(StartNodes);
            OnLeastSolutionCreated(leastSolution);
            return leastSolution;
        }

        protected virtual void OnLeastSolutionCreated(MinimalSpanningTree leastSolution)
        {
        }

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
                var hillClimber = new HillClimber(FitnessFunction, FixedNodes, SearchGraph.NodeDict.Values);
                BestSolution = hillClimber.Improve(BestSolution);
            }
        }

        private HashSet<ushort> SpannedMstToSkillnodes(MinimalSpanningTree mst)
        {
            if (!mst.IsSpanned)
                throw new Exception("The passed MST is not spanned!");

            var newSkilledNodes = new HashSet<ushort>(mst.UsedNodes);
            newSkilledNodes.UnionWith(StartNodes.Nodes.Select(node => node.Id));
            return newSkilledNodes;
        }

        private MinimalSpanningTree DnaToMst(BitArray dna)
        {
            var mstNodes = new List<GraphNode>();
            for (var i = 0; i < dna.Length; i++)
            {
                if (dna[i])
                    mstNodes.Add(SearchSpace[i]);
            }

            mstNodes.Add(StartNodes);
            mstNodes.AddRange(TargetNodes);

            return new MinimalSpanningTree(mstNodes, Distances);
        }

        private double FitnessFunction(BitArray dna)
        {
            var mst = DnaToMst(dna);
            mst.Span(StartNodes);
            return FitnessFunction(mst.UsedNodes);
        }

        protected abstract double FitnessFunction(HashSet<ushort> skilledNodes);
    }
}