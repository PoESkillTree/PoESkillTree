using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.TreeGenerator.Algorithm.Model;
using PoESkillTree.TreeGenerator.Algorithm.SteinerReductions;

namespace PoESkillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Reduces the search space of instances of the Steiner tree problem (SteinerSolver) and the variation used
    /// in AdvancedSolver to speed up runtime and increase the likelihood of finding the optimal solution.
    /// </summary>
    public class SteinerPreprocessor
    {
        /* Remarks for extensions that are not mentioned elsewhere:
         * - Nodes that are not fixed target nodes must only be merged into fixed target nodes.
         *   Because of this, a GraphNode only has nodes.Count > 1 if it is a fixed target node.
         * - Adjacency information (edges) between GraphNode are stored in _edgeSet. The information
         *   stored in the GraphNode must not be used once _edgeSet is initialised.
         */

        /// <summary>
        /// Contains the search space and target node states.
        /// </summary>
        private readonly NodeStates _nodeStates;

        /// <summary>
        /// Contains data about the current edges, distances, Steiner distances and start node.
        /// </summary>
        private readonly Data _data;

        /// <summary>
        /// Creates a new object that can reduce the problem instance described by the given parameters.
        /// </summary>
        /// <param name="searchSpace">Problem solutions must be a subset of these nodes. They also describe the initial edges.
        /// Must contain all nodes from the other parameters.</param>
        /// <param name="fixedTargetNodes">All nodes that must be included in solutions. The nodes for which the minimal
        /// Steiner tree has to be found.</param>
        /// <param name="startNode">The node from which solution trees are built. Must be a fixed target node.
        /// If null, the first fixed target node will be taken.</param>
        /// <param name="variableTargetNodes">Finding the subset of these nodes (unioned with the fixed target nodes) whose
        /// minimal Steiner tree optimizes the constraints formulates the extended Steiner problem solved by the AdvancedSolver.
        /// These nodes are in a special state between normal nodes and fixed target nodes for reductions.
        /// If null, no nodes are considered as variable target nodes and the problem is an instance of the normal Steiner tree problem.</param>
        /// <remarks>
        /// The fixed target nodes are removed from the search space and then added to the end of it so they always get the last distance
        /// indices. This improves the quality of solutions because the MST algorithm considers edges involving these after same priority edges
        /// not involving them.
        /// </remarks>
        public SteinerPreprocessor(IEnumerable<GraphNode> searchSpace, IEnumerable<GraphNode> fixedTargetNodes,
            GraphNode startNode = null, IEnumerable<GraphNode> variableTargetNodes = null)
        {
            var fixedTargetNodesSet = new HashSet<GraphNode>(fixedTargetNodes);
            if (!fixedTargetNodesSet.Any())
                throw new ArgumentException("At least one fixed target node must be provided.", "fixedTargetNodes");

            // Fixed target nodes are added last into the search space to get the last distance indices.
            _nodeStates = new NodeStates(searchSpace.Except(fixedTargetNodesSet).Union(fixedTargetNodesSet),
                fixedTargetNodesSet, variableTargetNodes ?? new GraphNode[0]);
            _data = new Data(startNode ?? _nodeStates.FixedTargetNodes.First());
            if (startNode != null && !_nodeStates.IsFixedTarget(startNode))
                throw new ArgumentException("Start node must be a fixed target node if specified.", "startNode");
        }
        
        /// <summary>
        /// Reduces the search space by removing nodes, removing edges and merging nodes.
        /// </summary>
        /// <returns>The nodes contained in the reduced search space.</returns>
        public SteinerPreprocessorResult ReduceSearchSpace()
        {
            // Remove all non target nodes with 2 or less edges. (Steiner nodes always have more than 2 edges)
            var nodeCountBefore = _nodeStates.SearchSpaceSize;
            _nodeStates.SearchSpace = _nodeStates.SearchSpace.Where(n => n.Adjacent.Count > 2 || _nodeStates.IsTarget(n)).ToList();
            Debug.WriteLine("First basic degree reduction:\n" +
                            "   removed nodes: " + (nodeCountBefore - _nodeStates.SearchSpaceSize) + " (of " + nodeCountBefore +
                            " initial nodes)");
            // Initialise the distance lookup.
            _data.DistanceCalculator = new DistanceCalculator(_nodeStates.SearchSpace);
            var distanceLookup = _data.DistanceCalculator;
            // Distance indices were not set before.
            _nodeStates.ComputeFields();

            // If a fixed target node is not connected to the start node, there obviously is no solution at all.
            if (_nodeStates.FixedTargetNodeIndices.Any(i => !distanceLookup.AreConnected(i, _data.StartNodeIndex)))
            {
                throw new GraphNotConnectedException();
            }

            nodeCountBefore = _nodeStates.SearchSpaceSize;
            // Remove all unconnected nodes.
            _nodeStates.SearchSpace = distanceLookup.RemoveNodes(_nodeStates.SearchSpace.Where(n => !distanceLookup.AreConnected(n, _data.StartNode)));
            Debug.WriteLine("Removed unconnected nodes:");
            Debug.WriteLine("   removed nodes: " + (nodeCountBefore - _nodeStates.SearchSpaceSize));

            // Even the basic degree reductions hurt the AdvancedSolver's performance.
            // Reenabling should be tested when other algorithm parts are changed/improved.
            // todo experiment with reductions and AdvancedSolver
            if (_nodeStates.VariableTargetNodeCount > 0)
            {
                return CreateResult();
            }

            // Initialise node states and edges. Edges are calculated from the information in the
            // GraphNode. Adjacency information from GraphNode instances must not be used after this.
            _data.EdgeSet = ComputeEdges();
            var initialNodeCount = _nodeStates.SearchSpaceSize;
            var initialEdgeCount = _data.EdgeSet.Count;
            Debug.WriteLine("Initial counts:\n"
                + "             nodes: " + initialNodeCount + "\n"
                + "       non targets: " + (_nodeStates.SearchSpaceSize - _nodeStates.TargetNodeCount) + "\n"
                + "  variable targets: " + _nodeStates.VariableTargetNodeCount + "\n"
                + "     fixed targets: " + _nodeStates.FixedTargetNodeCount + "\n"
                + "             edges: " + initialEdgeCount);

            // Execute an initial DegreeTest.
            var degreeTest = new DegreeTest(_nodeStates, _data);
            var dummy = 0;
            degreeTest.RunTest(ref dummy, ref dummy);

            // Update _searchSpace, _edgeSet and _distanceLookup
            ContractSearchSpace();
            // These values may become lower by merging nodes. Since the reductions based on these distance
            // don't help if there are many variable target nodes, it is not really worth it to always recalculate them.
            // It would either slow the preprocessing by like 30% or would need an approximation algorithm.
            _data.SMatrix = new BottleneckSteinerDistanceCalculator(distanceLookup.DistanceLookup)
                .CalcBottleneckSteinerDistances(_nodeStates.FixedTargetNodeIndices);

            // The set of reduction test that are run until they are no longer able to reduce the search space.
            var tests = new List<SteinerReduction>
            {
                new FarAwayNonTerminalsTest(_nodeStates, _data),
                new PathsWithManyTerminalsTest(_nodeStates, _data),
                new NonTerminalsOfDegreeKTest(_nodeStates, _data),
                new NearestVertexTest(_nodeStates, _data)
            };
            // Run every reduction test (each followed by a simple degree reduction test) until they are no longer able
            // to reduce the search space or 10 reduction rounds were executed.
            // (the 10 round limit is never actually reached from what I've seen)
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

                // No test should be still enabled in this case so we can stop running them.
                if (edgeElims == 0) break;
            }

            // Calculate the final search space.
            ContractSearchSpace();

            Debug.WriteLine("Final counts:\n"
                + "             nodes: " + _nodeStates.SearchSpaceSize + " (of " + initialNodeCount + " after basic reductions)\n"
                + "       non targets: " + (_nodeStates.SearchSpaceSize - _nodeStates.TargetNodeCount) + "\n"
                + "  variable targets: " + _nodeStates.VariableTargetNodeCount + "\n"
                + "     fixed targets: " + _nodeStates.FixedTargetNodeCount + "\n"
                + "             edges: " + _data.EdgeSet.Count + " (of " + initialEdgeCount + " after basic reductions)");

            return CreateResult();
        }

        /// <summary>
        /// Removes all nodes that were marked as removed from the search space and updates the distance lookup and the edge set.
        /// </summary>
        private void ContractSearchSpace()
        {
            var distanceLookup = _data.DistanceCalculator;
            // Save all edges by the GraphNodes they are connecting. (the indices are worthless after the search space is contracted)
            var edges =
                _data.EdgeSet.Select(
                    e => Tuple.Create(distanceLookup.IndexToNode(e.N1), distanceLookup.IndexToNode(e.N2), e.Weight))
                    .ToList();
            // Contract the search space and update _distanceLookup.
            _nodeStates.SearchSpace = distanceLookup.RemoveNodes(_nodeStates.SearchSpace.Where(n => _nodeStates.IsRemoved(n.DistancesIndex)));
            // Add all edges back.
            _data.EdgeSet = new GraphEdgeSet(distanceLookup.CacheSize);
            foreach (var tuple in edges)
            {
                _data.EdgeSet.Add(tuple.Item1.DistancesIndex, tuple.Item2.DistancesIndex, tuple.Item3);
            }
        }

        /// <summary>
        /// Calculates the edge set from the adjacency information stored in the GraphNodes of the current search space.
        /// </summary>
        private GraphEdgeSet ComputeEdges()
        {
            var edgeSet = new GraphEdgeSet(_nodeStates.SearchSpaceSize);
            // Go through all nodes and their neigbors ...
            foreach (var node in _nodeStates.SearchSpace)
            {
                foreach (var neighbor in node.Adjacent)
                {
                    var current = neighbor;
                    var previous = node;
                    var path = new HashSet<ushort>();
                    // Skip nodes with degree 2 that are not target nodes and instead traverse through them.
                    while (current.Adjacent.Count == 2 && (current.DistancesIndex < 0 || !_nodeStates.IsTarget(current.DistancesIndex)))
                    {
                        path.Add(current.Id);
                        var tmp = current;
                        // Because the current node has degree 2, there is exactly one node adjacent that is not the previous one.
                        current = current.Adjacent.First(n => n != previous);
                        previous = tmp;
                    }
                    // Now create an edge if
                    // - current is in the search space
                    // - the edge is not reflexive
                    // - the edge is the shortest path between both nodes
                    if (current.DistancesIndex >= 0 && node != current &&
                        path.SetEquals(_data.DistanceCalculator.GetShortestPath(node.DistancesIndex, current.DistancesIndex)))
                    {
                        edgeSet.Add(node.DistancesIndex, current.DistancesIndex,
                            _data.DistanceCalculator[node.DistancesIndex, current.DistancesIndex]);
                    }
                }
            }
            return edgeSet;
        }

        private SteinerPreprocessorResult CreateResult()
            => new SteinerPreprocessorResult(
                _nodeStates.FixedTargetNodes.ToList(),
                _data.StartNode,
                _nodeStates.SearchSpace,
                _data.DistanceCalculator.DistanceLookup,
                _data.DistanceCalculator.ShortestPathLookup);
    }

    public class SteinerPreprocessorResult
    {
        /// <summary>
        /// Gets the target nodes of this problem instance. These are the subset of target nodes provided
        /// at construction which remained in the search space.
        /// </summary>
        public IReadOnlyList<GraphNode> FixedTargetNodes { get; }

        /// <summary>
        /// Gets the GraphNode which serves as the start node from which trees are built.
        /// 
        /// <code>StartNode.Nodes</code> always contains the initial start node.
        /// </summary>
        public GraphNode StartNode { get; }

        /// <summary>
        /// Gets the nodes in the reduced search space.
        /// </summary>
        public IReadOnlyList<GraphNode> RemainingNodes { get; }

        /// <summary>
        /// Gets a distance lookup containing distances between all nodes of the search space.
        /// </summary>
        public DistanceLookup DistanceLookup { get; }

        /// <summary>
        /// Gets a distance lookup containing shortest paths between all nodes of the search space.
        /// </summary>
        public ShortestPathLookup ShortestPathLookup { get; }

        public SteinerPreprocessorResult(
            IReadOnlyList<GraphNode> fixedTargetNodes, GraphNode startNode, IReadOnlyList<GraphNode> remainingNodes,
            DistanceLookup distanceLookup, ShortestPathLookup shortestPathLookup)
        {
            FixedTargetNodes = fixedTargetNodes;
            StartNode = startNode;
            RemainingNodes = remainingNodes;
            DistanceLookup = distanceLookup;
            ShortestPathLookup = shortestPathLookup;
        }
    }
}