using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.SteinerReductions;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public class SteinerPreprocessor
    {
        private List<GraphNode> _searchSpace;

        private readonly HashSet<GraphNode> _fixedTargetNodes;

        public IReadOnlyList<GraphNode> FixedTargetNodes
        {
            get { return _fixedTargetNodes.ToList(); }
        }

        private readonly HashSet<GraphNode> _variableTargetNodes;

        private readonly HashSet<GraphNode> _allTargetNodes;

        public GraphNode StartNode { get; private set; }

        private DistanceLookup _distanceLookup;

        public IDistancePathLookup DistanceLookup { get { return _distanceLookup; } }

        private GraphEdgeSet _edgeSet;

        public IReadOnlyGraphEdgeSet EdgeSet { get { return _edgeSet; } }

        private NodeStates _nodeStates;

        private Data _data;

        public SteinerPreprocessor(IEnumerable<GraphNode> searchSpace, IEnumerable<GraphNode> fixedTargetNodes,
            GraphNode startNode = null, IEnumerable<GraphNode> variableTargetNodes = null)
        {
            _fixedTargetNodes = new HashSet<GraphNode>(fixedTargetNodes);
            if (!_fixedTargetNodes.Any())
                throw new ArgumentException("At least one fixed target node must be provided.", "fixedTargetNodes");
            _variableTargetNodes = new HashSet<GraphNode>(variableTargetNodes ?? new GraphNode[0]);
            _allTargetNodes = new HashSet<GraphNode>(_variableTargetNodes.Union(_fixedTargetNodes));

            // Fixed target nodes are added last into the search space to get the last distance indexes.
            // TODO make it adjustable? (at least document it in method)
            _searchSpace = searchSpace.Except(_fixedTargetNodes).Union(_fixedTargetNodes).ToList();
            StartNode = startNode ?? _fixedTargetNodes.First();
            if (!_fixedTargetNodes.Contains(StartNode))
                throw new ArgumentException("Start node must be a fixed target node if specified.", "startNode");
        }
        
        // Nodes that are not fixed target nodes must only be merged into fixed target nodes.
        // A GraphNode either has nodes.Count = 1 or is a fixed target node.
        public List<GraphNode> ReduceSearchSpace()
        {
            // Basic reduction based on steiner nodes needing more than two edges.
            var nodeCountBefore = _searchSpace.Count;
            _searchSpace = _searchSpace.Where(n => n.Adjacent.Count > 2 || _allTargetNodes.Contains(n)).ToList();
            Debug.WriteLine("First basic degree reduction:\n" +
                            "   removed nodes: " + (nodeCountBefore - _searchSpace.Count) + " (of " + nodeCountBefore +
                            " initial nodes)");
            _distanceLookup = new DistanceLookup(_searchSpace);

            if (_fixedTargetNodes.Any(n => !_distanceLookup.AreConnected(n, StartNode)))
            {
                throw new GraphNotConnectedException();
            }

            nodeCountBefore = _searchSpace.Count;
            // Removal of unconnected nodes.
            _searchSpace = _distanceLookup.RemoveNodes(_searchSpace.Where(n => !_distanceLookup.AreConnected(n, StartNode)));
            Debug.WriteLine("Removed unconnected nodes:");
            Debug.WriteLine("   removed nodes: " + (nodeCountBefore - _searchSpace.Count));

            // Even the basic degree reductions hurt the AdvancedSolver's performance.
            // Reenabling should be tested when other algorithm parts are changed/improved.
            if (_variableTargetNodes.Count > 0)
            {
                return _searchSpace;
            }

            _nodeStates = new NodeStates(_searchSpace, _fixedTargetNodes, _variableTargetNodes, _allTargetNodes);
            _edgeSet = ComputeEdges();
            var initialNodeCount = _searchSpace.Count;
            var initialEdgeCount = _edgeSet.Count;
            Debug.WriteLine("Initial counts:\n"
                + "             nodes: " + initialNodeCount + "\n"
                + "       non targets: " + (_searchSpace.Count - _allTargetNodes.Count) + "\n"
                + "  variable targets: " + _variableTargetNodes.Count + "\n"
                + "     fixed targets: " + _fixedTargetNodes.Count + "\n"
                + "             edges: " + initialEdgeCount);

            _data = new Data(_edgeSet, _distanceLookup, StartNode);
            _data.StartNodeChanged += (sender, node) => StartNode = node;
            var dummy = 0;
            new DegreeTest(_nodeStates, _data).RunTest(ref dummy, ref dummy);

            ContractSearchSpace();
            // These values may become lower by merging nodes. Since the reductions based on these distance
            // don't help if there are many variable target nodes, it is not really worth it to always recalculate them.
            // It would either slow the preprocessing by like 30% or would need an approximation algorithm.
            _data.SMatrix = new BottleneckSteinerDistanceCalculator(_distanceLookup).CalcBottleneckSteinerDistances(_fixedTargetNodes);

            var degreeTest = new DegreeTest(_nodeStates, _data);
            var tests = new List<SteinerReduction>
            {
                new FarAwayNonTerminalsTest(_nodeStates, _data) { IsEnabled = _variableTargetNodes.Count == 0 },
                new PathsWithManyTerminalsTest(_nodeStates, _data),
                new NonTerminalsOfDegreeKTest(_nodeStates, _data),
                new NearestVertexTest(_nodeStates, _data),
                new ShortestLinksTest(_nodeStates, _data)
            };
            for (int i = 0; i < 10; i++)
            {
                var edgeElims = 0;
                var nodeElims = 0;
                
                foreach (var test in tests.Where(t => t.IsEnabled))
                {
                    if (!test.RunTest(ref edgeElims, ref nodeElims))
                    {
                        test.IsEnabled = false;
                    }
                    degreeTest.RunTest(ref edgeElims, ref nodeElims);
                }

                Debug.WriteLine("Eliminations in round {0}:", i + 1);
                Debug.WriteLine("   removed nodes: " + nodeElims);
                Debug.WriteLine("   removed edges: " + edgeElims);

                if (edgeElims == 0) break;
            }

            ContractSearchSpace();

            Debug.WriteLine("Final counts:\n"
                + "             nodes: " + _searchSpace.Count + " (of " + initialNodeCount + " after basic reductions)\n"
                + "       non targets: " + (_searchSpace.Count - _allTargetNodes.Count) + "\n"
                + "  variable targets: " + _variableTargetNodes.Count + "\n"
                + "     fixed targets: " + _fixedTargetNodes.Count + "\n"
                + "             edges: " + _edgeSet.Count + " (of " + initialEdgeCount + " after basic reductions)");

            return _searchSpace;
        }

        private void ContractSearchSpace()
        {
            var edges =
                _edgeSet.Select(
                    e => Tuple.Create(_distanceLookup.IndexToNode(e.N1), _distanceLookup.IndexToNode(e.N2), e.Weight))
                    .ToList();
            _searchSpace = _distanceLookup.RemoveNodes(_searchSpace.Where(n => _nodeStates.IsRemoved(n.DistancesIndex)));
            _edgeSet = new GraphEdgeSet(_distanceLookup.CacheSize);
            foreach (var tuple in edges)
            {
                _edgeSet.Add(tuple.Item1.DistancesIndex, tuple.Item2.DistancesIndex, tuple.Item3);
            }

            _nodeStates.SearchSpace = _searchSpace;
            _data.EdgeSet = _edgeSet;
        }

        private GraphEdgeSet ComputeEdges()
        {
            var edgeSet = new GraphEdgeSet(_searchSpace.Count);
            foreach (var node in _searchSpace)
            {
                foreach (var neighbor in node.Adjacent)
                {
                    var current = neighbor;
                    var previous = node;
                    var path = new HashSet<ushort>();
                    while (current.Adjacent.Count == 2 && (current.DistancesIndex < 0 || !_nodeStates.IsTarget(current.DistancesIndex)))
                    {
                        path.Add(current.Id);
                        var tmp = current;
                        current = current.Adjacent.First(n => n != previous);
                        previous = tmp;
                    }
                    // Only edges representing the shortest path are kept.
                    if (current.DistancesIndex >= 0 && node != current &&
                        path.SetEquals(_distanceLookup.GetShortestPath(node.DistancesIndex, current.DistancesIndex)))
                    {
                        edgeSet.Add(node.DistancesIndex, current.DistancesIndex,
                            _distanceLookup[node.DistancesIndex, current.DistancesIndex]);
                    }
                }
            }
            return edgeSet;
        }
        
    }
}