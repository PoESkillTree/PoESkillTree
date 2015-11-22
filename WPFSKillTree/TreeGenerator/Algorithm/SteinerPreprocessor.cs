using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public class SteinerPreprocessor
    {
        private class SMatrixLookup : IDistanceLookup
        {
            public int CacheSize { get; private set; }

            private readonly uint[,] _smatrix;

            public uint this[int a, int b]
            {
                get { return _smatrix[a, b]; }
            }

            public SMatrixLookup(int cacheSize, uint[,] smatrix)
            {
                CacheSize = cacheSize;
                _smatrix = smatrix;
            }
        }

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
        
        //public IEnumerable<ushort> LeastSolution { get; private set; }

        private GraphEdgeSet _edgeSet;

        public IReadOnlyGraphEdgeSet EdgeSet { get { return _edgeSet; } }

        private bool[] _isFixedTarget;

        private bool[] _isVariableTarget;

        private bool[] _isTarget;

        private bool[] _isRemoved;

        private uint[,] _smatrix;

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
            // Basis reduction based on steiner nodes needing more than two edges.
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

            ComputeFields();
            _edgeSet = ComputeEdges();
            var initialNodeCount = _searchSpace.Count;
            var initialEdgeCount = _edgeSet.Count;
            Debug.WriteLine("Initial counts:\n"
                + "             nodes: " + initialNodeCount + "\n"
                + "       non targets: " + (_searchSpace.Count - _allTargetNodes.Count) + "\n"
                + "  variable targets: " + _variableTargetNodes.Count + "\n"
                + "     fixed targets: " + _fixedTargetNodes.Count + "\n"
                + "             edges: " + initialEdgeCount);
            
            var dummy = 0;
            RunDegreeTest("0", ref dummy, ref dummy);

            ContractSearchSpace();
            ComputeFields();

            // These values may become lower by merging nodes. Since the reductions based on these distance
            // don't help if there are many variable target nodes, it is not really worth it to always recalculate them.
            // It would either slow the preprocessing by like 30% or would need an approximation algorithm.
            _smatrix = CalcBottleneckSteinerDistances();

            var farAwayNonTerminalsEnabled    = _variableTargetNodes.Count == 0;
            var pathsWithManyTerminalsEnabled = true;
            var nonTermimalsOfDegreeKEnabled  = true;
            var nearestVertexEnabled          = true;
            var shortestLinksEnabled          = true;
            for (var i = 1; i < 10; i++)
            {
                var edgeElims = 0;
                var nodeElims = 0;

                if (farAwayNonTerminalsEnabled)
                {
                    if (!RunTest(FarAwayNonTerminalsTest, "Far away non terminals", i.ToString(),
                        ref edgeElims, ref nodeElims))
                    {
                        farAwayNonTerminalsEnabled = false;
                    }
                    RunDegreeTest(i + ".0", ref edgeElims, ref nodeElims);
                }

                if (pathsWithManyTerminalsEnabled)
                {
                    if (!RunTest(PathsWithManyTerminalsTest, "Paths with many Terminals", i.ToString(),
                        ref edgeElims, ref nodeElims))
                    {
                        pathsWithManyTerminalsEnabled = false;
                    }
                    RunDegreeTest(i + ".1", ref edgeElims, ref nodeElims);
                }

                if (nonTermimalsOfDegreeKEnabled)
                {
                    if (!RunTest(NonTerminalsOfDegreeKTest, "Non Terminals of Degree k", i.ToString(),
                        ref edgeElims, ref nodeElims))
                    {
                        nonTermimalsOfDegreeKEnabled = false;
                    }
                    RunDegreeTest(i + ".2", ref edgeElims, ref nodeElims);
                }

                if (nearestVertexEnabled)
                {
                    if (!RunTest(NearestVertextTest, "Nearest Vertex", i.ToString(),
                        ref edgeElims, ref nodeElims))
                    {
                        nearestVertexEnabled = false;
                    }
                    RunDegreeTest(i + ".3", ref edgeElims, ref nodeElims);
                }

                if (shortestLinksEnabled)
                {
                    if (!RunTest(ShortLinksTest, "Shortest Links", i.ToString(),
                        ref edgeElims, ref nodeElims))
                    {
                        shortestLinksEnabled = false;
                    }
                    RunDegreeTest(i + ".4", ref edgeElims, ref nodeElims);
                }

                Debug.WriteLine("Eliminations in round {0}:", i);
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

        private void RunDegreeTest(string iterationId, ref int edgeElims, ref int nodeElims)
        {
            RunTest(DegreeTest, "Degree", iterationId, ref edgeElims, ref nodeElims);
        }

        private bool RunTest(Func<int> testFunc, string testId, string iterationId,
            ref int edgeElims, ref int nodeElims)
        {
            var edgeCountBefore = _edgeSet.Count;
            var removedNodes = testFunc();
            Debug.WriteLine("{0} Test #{1}:", testId, iterationId);
            Debug.WriteLine("   removed nodes: " + removedNodes);
            Debug.WriteLine("   removed edges: " + (edgeCountBefore - _edgeSet.Count));
            edgeElims += edgeCountBefore - _edgeSet.Count;
            nodeElims += removedNodes;
            return edgeCountBefore - _edgeSet.Count > 0;
        }

        private void ComputeFields()
        {
            var searchSpaceIndexes = SearchSpaceIndexes().ToList();
            _isFixedTarget = 
                searchSpaceIndexes.Select(i => _fixedTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isVariableTarget =
                searchSpaceIndexes.Select(i => _variableTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isTarget = searchSpaceIndexes.Select(i => _isFixedTarget[i] || _isVariableTarget[i]).ToArray();
            _isRemoved = new bool[_searchSpace.Count];
        }

        private void ContractSearchSpace()
        {
            var edges =
                _edgeSet.Select(
                    e => Tuple.Create(_distanceLookup.IndexToNode(e.N1), _distanceLookup.IndexToNode(e.N2), e.Weight))
                    .ToList();
            _searchSpace = _distanceLookup.RemoveNodes(_searchSpace.Where(n => _isRemoved[n.DistancesIndex]));
            _edgeSet = new GraphEdgeSet(_distanceLookup.CacheSize);
            foreach (var tuple in edges)
            {
                _edgeSet.Add(tuple.Item1.DistancesIndex, tuple.Item2.DistancesIndex, tuple.Item3);
            }
        }

        private IEnumerable<int> SearchSpaceIndexes()
        {
            return Enumerable.Range(0, _searchSpace.Count);
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
                    while (current.Adjacent.Count == 2 && (current.DistancesIndex < 0 || !_isTarget[current.DistancesIndex]))
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

        private int DegreeTest()
        {
            var removedNodes = 0;

            var untested = new HashSet<int>(SearchSpaceIndexes());
            var dependentNodes = new Dictionary<int, List<int>>();
            while (untested.Any())
            {
                var i = untested.First();
                untested.Remove(i);
                
                var neighbors = _edgeSet.NeighborsOf(i);
                
                if (!_isTarget[i])
                {
                    if (neighbors.Count == 1)
                    {
                        // Non target nodes with one neighbor can be removed.
                        untested.Add(neighbors[0]);
                        RemoveNode(i);
                        removedNodes++;
                    }
                    else if (neighbors.Count == 2)
                    {
                        // Non target nodes with two neighbors can be removed and their neighbors
                        // connected directly.
                        untested.Add(neighbors[0]);
                        untested.Add(neighbors[1]);
                        RemoveNode(i);
                        removedNodes++;
                    }
                }
                else if (_isFixedTarget[i])
                {
                    if (neighbors.Count == 1)
                    {
                        var other = neighbors[0];
                        if (_edgeSet.NeighborsOf(other).Count > 2 || _isTarget[other])
                        {
                            // Fixed target nodes with one neighbor can be merged with their neighbor since
                            // it must always be taken.
                            untested.Add(i);
                            untested.Remove(other);
                            untested.UnionWith(MergeInto(other, i));
                            removedNodes++;
                        }
                        else
                        {
                            // Node can only be merged once other has been processed. Other might be a dead end.
                            Debug.Assert(untested.Contains(other));
                            dependentNodes.Add(other, i);
                        }
                    }
                    else if (neighbors.Count > 1)
                    {
                        // Edges from one target node to another that are of minimum cost among the edges of
                        // one of the nodes can be in any optimal solution. Therefore both target nodes can be merged.
                        var minimumEdgeCost = neighbors.Min(other => _distanceLookup[i, other]);
                        var minimumTargetNeighbors =
                            neighbors.Where(other => _distanceLookup[i, other] == minimumEdgeCost && _isFixedTarget[other]);
                        foreach (var other in minimumTargetNeighbors)
                        {
                            untested.Add(i);
                            untested.Remove(other);
                            untested.UnionWith(MergeInto(other, i));
                            removedNodes++;
                        }
                    }
                }

                List<int> dependent;
                if (dependentNodes.TryGetValue(i, out dependent))
                {
                    untested.UnionWith(dependent);
                }
            }
            
            return removedNodes;
        }

        private uint[] CalcBottleneckSteinerDistancesTo(int from, IEnumerable<int> to)
        {
            var nodeCount = _searchSpace.Count;

            // All not permanetly labeled target nodes.
            var targetsRemaining = new HashSet<int>(_fixedTargetNodes.Select(n => n.DistancesIndex));
            targetsRemaining.Remove(from);
            // All not permanently labeled nodes (neigbors and targets, we don't care about other nodes).
            var nodesRemaining = new HashSet<int>(targetsRemaining);
            nodesRemaining.UnionWith(to);

            // Labels of nodes, approaches special distance to i.
            var labels = new uint[nodeCount];
            foreach (var j in nodesRemaining)
            {
                // Initialize labels with distance.
                labels[j] = _distanceLookup[from, j];
            }

            // While not all target nodes were labeled.
            while (targetsRemaining.Any())
            {
                // Determine the not permanently labeled target node k* with smallest label and select it.
                var max = uint.MaxValue;
                var kstar = -1;
                foreach (var t in targetsRemaining)
                {
                    if (labels[t] < max)
                    {
                        max = labels[t];
                        kstar = t;
                    }
                }
                // Label k* permanently.
                targetsRemaining.Remove(kstar);
                nodesRemaining.Remove(kstar);

                // For all not permanently labeled nodes.
                foreach (var j in nodesRemaining)
                {
                    // Decrease label if the bottleneck length on the path over k* is smaller.
                    labels[j] = Math.Min(labels[j], Math.Max(labels[kstar], _distanceLookup[kstar, j]));
                }
            }

            return labels;
        }

        private uint[,] CalcBottleneckSteinerDistances()
        {
            var nodeCount = _searchSpace.Count;
            var smatrix = new uint[nodeCount, nodeCount];
            // For each node i
            for (var i1 = 0; i1 < nodeCount; i1++)
            {
                var labels = CalcBottleneckSteinerDistancesTo(i1, SearchSpaceIndexes());

                foreach (var j in SearchSpaceIndexes())
                {
                    smatrix[i1, j] = labels[j];
                }
            }
            return smatrix;
        }

        private int PathsWithManyTerminalsTest()
        {
            _edgeSet.Where(e => e.Weight > _smatrix[e.N1, e.N2])
                .ToList().ForEach(_edgeSet.Remove);

            return 0;
        }

        private int FarAwayNonTerminalsTest()
        {
            if (_fixedTargetNodes.Count <= 1 || _variableTargetNodes.Count > 0) return 0;

            var removedNodes = 0;

            var mst = new MinimalSpanningTree(_fixedTargetNodes.Select(n => n.DistancesIndex).ToList(), _distanceLookup);
            mst.Span(StartNode.DistancesIndex);
            var maxEdgeDistance = mst.SpanningEdges.Max(e => _distanceLookup[e.Inside, e.Outside]);

            var voronoiPartition = new VoronoiPartition(_distanceLookup, _fixedTargetNodes.Select(n => n.DistancesIndex), _edgeSet);

            for (var i = 0; i < _searchSpace.Count; i++)
            {
                if (_isTarget[i] || _isRemoved[i]) continue;

                if (_distanceLookup[i, voronoiPartition.Base(i)] >= maxEdgeDistance)
                {
                    _edgeSet.EdgesOf(i).ForEach(_edgeSet.Remove);
                    MarkNodeAsRemoved(i);
                    removedNodes++;
                }
            }

            return removedNodes;
        }

        private int NonTerminalsOfDegreeKTest()
        {
            var edges = new GraphEdge[_searchSpace.Count];
            var removedNodes = 0;
            for (var i = 0; i < _searchSpace.Count; i++)
            {
                var neighbors = _edgeSet.NeighborsOf(i);
                if (neighbors.Count < 3 || neighbors.Count > 6 || _isTarget[i]) continue;

                var maxIndex = i;
                foreach (var neighbor in neighbors)
                {
                    edges[neighbor] = _edgeSet[i, neighbor];
                    if (neighbor > maxIndex)
                    {
                        maxIndex = neighbor;
                    }
                }

                var canBeRemoved = true;
                foreach (var subset in GetAllSubsets(neighbors))
                {
                    if (subset.Count < 3) continue;
                    
                    var edgeSum = subset.Sum(j => edges[j].Weight);
                    var mst = new MinimalSpanningTree(subset, new SMatrixLookup(maxIndex + 1, _smatrix));
                    mst.Span(subset[0]);
                    var mstEdgeSum = mst.SpanningEdges.Sum(e => _smatrix[e.Inside, e.Outside]);
                    if (edgeSum < mstEdgeSum)
                    {
                        canBeRemoved = false;
                        break;
                    }
                }

                if (!canBeRemoved) continue;

                foreach (var neighbor in neighbors)
                {
                    var edge = edges[neighbor];
                    _edgeSet.Remove(edge);
                    foreach (var neighbor2 in neighbors)
                    {
                        if (neighbor >= neighbor2) continue;
                        var edge2 = edges[neighbor2];
                        var newEdgeWeight = edge.Weight + edge2.Weight;
                        if (newEdgeWeight <= _smatrix[neighbor, neighbor2])
                        {
                            _edgeSet.Add(neighbor, neighbor2, newEdgeWeight);
                        }
                    }
                }

                MarkNodeAsRemoved(i);
                removedNodes++;
            }
            return removedNodes;
        }

        private static IEnumerable<List<int>> GetAllSubsets(IReadOnlyList<int> of)
        {
            var subsets = new List<List<int>>((int)Math.Pow(2, of.Count));
            for (var i = 1; i < of.Count; i++)
            {
                subsets.Add(new List<int>(new[] {of[i -1]}));
                var i1 = i;
                var newSubsets = subsets.Select(subset => subset.Concat(new[] {of[i1]}).ToList()).ToList();
                subsets.AddRange(newSubsets);
            }
            subsets.Add(new List<int>(new[] {of.Last()}));
            return subsets;
        }

        private int NearestVertextTest()
        {
            if (_fixedTargetNodes.Count == 1) return 0;

            var removedNodes = 0;
            var untested = new HashSet<int>(_fixedTargetNodes.Select(n => n.DistancesIndex));
            // For each terminal zi with degree of at least 2
            while (untested.Any())
            {
                var zi = untested.First();
                untested.Remove(zi);

                var neighbors = _edgeSet.NeighborsOf(zi);
                if (neighbors.Count < 2) continue;

                // Let (zi, v1) and (zi, v2) be the shortest and second shortest edges incident to zi.
                var tuple = ShortestTwoEdgesOf(_edgeSet.EdgesOf(zi));
                var shortest = tuple.Item1;
                var secondShortestWeight = tuple.Item2;

                var v1 = shortest.N1 == zi ? shortest.N2 : shortest.N1;
                
                // (zi, v1) belongs to at least one Steiner minimal tree, if there is a terminal zj, zi != zj
                // with: c(zi, v2) >= c(zi, v1) + d(v1, zj)
                var canBeContracted =
                    _fixedTargetNodes.Select(n => n.DistancesIndex)
                        .Where(zj => zi != zj)
                        .Any(zj => secondShortestWeight >= shortest.Weight + _distanceLookup[v1, zj]);
                // If there is, v1 and (zi, v1) can be merged into zi.
                if (canBeContracted)
                {
                    untested.Add(zi);
                    untested.Remove(v1);
                    MergeInto(v1, zi);
                    removedNodes++;
                }
            }
            return removedNodes;
        }

        private static Tuple<GraphEdge, uint> ShortestTwoEdgesOf(IReadOnlyList<GraphEdge> edges)
        {
            var shortest = edges[0];
            var secondShortestWeight = edges[1].Weight;
            if (shortest.Weight > secondShortestWeight)
            {
                secondShortestWeight = shortest.Weight;
                shortest = edges[1];
            }
            for (var i = 2; i < edges.Count; i++)
            {
                var currentWeight = edges[i].Weight;
                if (currentWeight < shortest.Weight)
                {
                    secondShortestWeight = shortest.Weight;
                    shortest = edges[i];
                }
                else if (currentWeight < secondShortestWeight)
                {
                    secondShortestWeight = currentWeight;
                }
            }
            return Tuple.Create(shortest, secondShortestWeight);
        }

        private int ShortLinksTest()
        {
            if (_fixedTargetNodes.Count == 1) return 0;

            var removedNodes = 0;

            var terminalVisited = new bool[_searchSpace.Count];
            var voronoiPartition = new VoronoiPartition(_distanceLookup, _fixedTargetNodes.Select(n => n.DistancesIndex),
                _edgeSet);

            for (var i = 0; i < _searchSpace.Count; i++)
            {
                if (!_isFixedTarget[i] || terminalVisited[i]) continue;

                var links = voronoiPartition.Links(i);
                var tuple = links.Count > 1 ? ShortestTwoEdgesOf(links) : Tuple.Create(links[0], uint.MaxValue);
                var shortestEdge = tuple.Item1;
                var secondShortestWeight = tuple.Item2;
                int iNonTerminal;
                int otherNonTerminal;
                if (voronoiPartition.Base(shortestEdge.N1) == i)
                {
                    iNonTerminal = shortestEdge.N1;
                    otherNonTerminal = shortestEdge.N2;
                }
                else
                {
                    iNonTerminal = shortestEdge.N2;
                    otherNonTerminal = shortestEdge.N1;
                }
                var otherTerminal = voronoiPartition.Base(otherNonTerminal);

                if (secondShortestWeight >=
                    _distanceLookup[iNonTerminal, i] + shortestEdge.Weight +
                    _distanceLookup[otherNonTerminal, otherTerminal])
                {
                    var into = _isFixedTarget[iNonTerminal]
                        ? iNonTerminal
                        : (_isTarget[otherNonTerminal] ? otherNonTerminal : iNonTerminal);
                    var x = into == iNonTerminal ? otherNonTerminal : iNonTerminal;
                    MakeFixedTarget(into);
                    MergeInto(x, into);
                    terminalVisited[i] = true;
                    terminalVisited[otherTerminal] = true;
                    // Was changed into a terminal, but was not considered in calculation of voronoiPartition.
                    terminalVisited[into] = true;
                    removedNodes++;
                }
            }

            return removedNodes;
        }

        private void RemoveNode(int index)
        {
            if (_isTarget[index])
                throw new ArgumentException("Target nodes can't be removed", "index");
            
            var neighbors = _edgeSet.NeighborsOf(index);
            switch (neighbors.Count)
            {
                case 0:
                    break;
                case 1:
                    _edgeSet.Remove(index, neighbors[0]);
                    break;
                case 2:
                    var left = neighbors[0];
                    var right = neighbors[1];
                    var newWeight = _edgeSet[index, left].Weight + _edgeSet[index, right].Weight;
                    _edgeSet.Remove(index, left);
                    _edgeSet.Remove(index, right);
                    if (newWeight <= _distanceLookup[left, right])
                    {
                        _edgeSet.Add(left, right, newWeight);
                    }
                    break;
                default:
                    throw new ArgumentException("Removing nodes with more than 2 neighbors is not supported", "index");
            }

            MarkNodeAsRemoved(index);
        }

        private IEnumerable<int> MergeInto(int x, int into)
        {
            if (!_isFixedTarget[into])
                throw new ArgumentException("Nodes can only be merged into fixed target nodes", "into");

            _searchSpace[into].MergeWith(_searchSpace[x], _distanceLookup.GetShortestPath(x, into));

            _distanceLookup.MergeInto(x, into);

            _edgeSet.Remove(x, into);
            var intoNeighbors = _edgeSet.NeighborsOf(into);
            var xNeighbors = _edgeSet.NeighborsOf(x);
            var neighbors = intoNeighbors.Union(xNeighbors);
            foreach (var neighbor in xNeighbors)
            {
                _edgeSet.Remove(x, neighbor);
            }
            foreach (var neighbor in neighbors)
            {
                _edgeSet.Add(into, neighbor, _distanceLookup[into, neighbor]);
            }

            if (StartNode.DistancesIndex == x)
            {
                StartNode = _distanceLookup.IndexToNode(into);
            }

            MarkNodeAsRemoved(x);

            return xNeighbors;
        }

        private void MakeFixedTarget(int i)
        {
            if (_isFixedTarget[i]) return;
            var node = _searchSpace[i];
            if (_isVariableTarget[i])
            {
                _isVariableTarget[i] = false;
                _variableTargetNodes.Remove(node);
            }
            else
            {
                _isTarget[i] = true;
            }
            _isFixedTarget[i] = true;
            _fixedTargetNodes.Add(node);
        }

        private void MarkNodeAsRemoved(int i)
        {
            _isRemoved[i] = true;
            if (_isTarget[i])
            {
                var node = _searchSpace[i];
                _isTarget[i] = false;
                _allTargetNodes.Remove(node);
                if (_isFixedTarget[i])
                {
                    _isFixedTarget[i] = false;
                    _fixedTargetNodes.Remove(node);
                }
                else
                {
                    _isVariableTarget[i] = false;
                    _variableTargetNodes.Remove(node);
                }
            }
        }
    }
}