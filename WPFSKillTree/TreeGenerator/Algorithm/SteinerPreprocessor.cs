using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm
{
    class GraphEdgeSet : ICollection<GraphEdge>
    {
        private readonly Dictionary<GraphEdge, GraphEdge> _edgeDict = new Dictionary<GraphEdge, GraphEdge>();

        private readonly HashSet<int>[] _adjacencyMatrix;

        public int Count { get { return _edgeDict.Count; } }

        public bool IsReadOnly { get { return false; } }

        public GraphEdge this[int n1, int n2]
        {
            get { return _edgeDict[CreateTmpEdge(n1, n2)]; }
        }

        public GraphEdgeSet(int nodeCount)
        {
            _adjacencyMatrix = Enumerable.Range(0, nodeCount).Select(_ => new HashSet<int>()).ToArray();
        }

        public IReadOnlyList<int> NeighborsOf(int node)
        {
            return _adjacencyMatrix[node].ToList();
        }

        public bool HasNeighbors(int node)
        {
            return _adjacencyMatrix[node].Any();
        }

        public void Add(GraphEdge edge)
        {
            _edgeDict[edge] = edge;
            _adjacencyMatrix[edge.N1].Add(edge.N2);
            _adjacencyMatrix[edge.N2].Add(edge.N1);
        }

        public void Clear()
        {
            foreach (var set in _adjacencyMatrix)
            {
                set.Clear();
            }
            _edgeDict.Clear();
        }

        public bool Contains(GraphEdge item)
        {
            return _edgeDict.ContainsKey(item);
        }

        public void CopyTo(GraphEdge[] array, int arrayIndex)
        {
            _edgeDict.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(GraphEdge edge)
        {
            _adjacencyMatrix[edge.N1].Remove(edge.N2);
            _adjacencyMatrix[edge.N2].Remove(edge.N1);
            return _edgeDict.Remove(edge);
        }

        public void Remove(int n1, int n2)
        {
            Remove(CreateTmpEdge(n1, n2));
        }

        private static GraphEdge CreateTmpEdge(int n1, int n2)
        {
            return new GraphEdge(n1, n2, 0);
        }

        public IEnumerator<GraphEdge> GetEnumerator()
        {
            return _edgeDict.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

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

        private readonly GraphNode _startNode;

        private readonly DistanceLookup _distanceLookup;

        public IDistanceLookup DistanceLookup { get { return _distanceLookup; } }
        
        public IEnumerable<ushort> LeastSolution { get; private set; }

        private GraphEdgeSet _edgeSet;

        private bool[] _isFixedTarget;

        private bool[] _isVariableTarget;

        private bool[] _isTarget;

        private bool[] _isRemoved;

        public SteinerPreprocessor(IEnumerable<GraphNode> searchSpace, IEnumerable<GraphNode> fixedTargetNodes,
            GraphNode startNode = null, IEnumerable<GraphNode> variableTargetNodes = null,
            DistanceLookup distanceLookup = null)
        {
            _fixedTargetNodes = new HashSet<GraphNode>(fixedTargetNodes);
            if (!_fixedTargetNodes.Any())
                throw new ArgumentException("At least one fixed target node must be provided.", "fixedTargetNodes");
            _variableTargetNodes = new HashSet<GraphNode>(variableTargetNodes ?? new GraphNode[0]);
            _allTargetNodes = new HashSet<GraphNode>(_variableTargetNodes.Union(_fixedTargetNodes));

            // Fixed target nodes are added last into the search space to get the last distance indexes.
            // TODO make it adjustable? (at least document it in method)
            _searchSpace = searchSpace.Except(_fixedTargetNodes).Union(_fixedTargetNodes).ToList();
            _startNode = startNode ?? _fixedTargetNodes.First();
            if (!_fixedTargetNodes.Contains(_startNode))
                throw new ArgumentException("Start node must be a fixed target node if specified.", "startNode");

            _distanceLookup = distanceLookup ?? new DistanceLookup();
        }
        
        // Nodes that are not fixed target nodes must only be merged into fixed target nodes.
        // A GraphNode either has nodes.Count = 1 or is a fixed target node.
        public List<GraphNode> ReduceSearchSpace()
        {
            // Basis reduction based on steiner nodes needing more than two edges.
            _searchSpace = _searchSpace.Where(n => n.Adjacent.Count > 2 || _allTargetNodes.Contains(n)).ToList();
            _distanceLookup.CalculateFully(_searchSpace);

            if (_fixedTargetNodes.Any(n => !_distanceLookup.AreConnected(n, _startNode)))
            {
                throw new GraphNotConnectedException();
            }
            
            var leastMst = new MinimalSpanningTree(_fixedTargetNodes.ToList(), _distanceLookup);
            leastMst.Span(_startNode);
            LeastSolution = leastMst.GetUsedNodes();

            // Removal of nodes which are not connected or too far away to be useful.
            // TODO check if maxEdgeDistance is of use when there are variable target nodes
            var maxEdgeDistance = _fixedTargetNodes.Count > 1 && _variableTargetNodes.Count == 0
                ? leastMst.SpanningEdges.Max(e => e.Weight)
                : int.MaxValue;
            var query =
                from n in _searchSpace
                where !_distanceLookup.AreConnected(n, _startNode) ||
                      (!_allTargetNodes.Contains(n) &&
                       _fixedTargetNodes.All(targetNode => _distanceLookup[targetNode, n] >= maxEdgeDistance))
                select n;
            _searchSpace = _distanceLookup.RemoveNodes(query);

            ComputeFields();

            DegreeTest();

            // TODO further reductions

            // TODO save edges somwhere for use from outside

            _searchSpace = _distanceLookup.RemoveNodes(_searchSpace.Where(n => _isRemoved[n.DistancesIndex]));

            return _searchSpace;
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
            _edgeSet = ComputeEdges();
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
                    if (current.DistancesIndex >= 0 && node != current &&
                        path.SetEquals(_distanceLookup.GetShortestPath(node.DistancesIndex, current.DistancesIndex)))
                    {
                        edgeSet.Add(new GraphEdge(node.DistancesIndex, current.DistancesIndex,
                            _distanceLookup[node.DistancesIndex, current.DistancesIndex]));
                    }
                }
            }
            return edgeSet;
        }

        private void DegreeTest()
        {
            var untested = new HashSet<int>(SearchSpaceIndexes());
            while (untested.Any())
            {
                var i = untested.First();
                untested.Remove(i);
                
                var neighbors = _edgeSet.NeighborsOf(i);
                if (!_isTarget[i])
                {
                    if (neighbors.Count == 1)
                    {
                        untested.Add(neighbors[0]);
                        RemoveNode(i);
                    }
                    else if (neighbors.Count == 2)
                    {
                        untested.Add(neighbors[0]);
                        untested.Add(neighbors[1]);
                        RemoveNode(i);
                    }
                }
                else if (_isFixedTarget[i] && neighbors.Any())
                {
                    if (neighbors.Count == 1)
                    {
                        untested.Add(i);
                        MergeInto(neighbors[0], i);
                        continue;
                    }

                    var minimumEdgeCost = neighbors.Min(other => _distanceLookup[i, other]);
                    var minimumTargetNeighbors =
                        neighbors.Where(other => _distanceLookup[i, other] == minimumEdgeCost && _isFixedTarget[other]);
                    foreach (var other in minimumTargetNeighbors)
                    {
                        untested.Add(i);
                        MergeInto(other, i);
                    }
                }
            }

            RemoveUnconnectedNodes();
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
                    if (_distanceLookup[left, right] >= newWeight)
                    {
                        _edgeSet.Add(new GraphEdge(left, right, newWeight));
                    }
                    break;
                default:
                    throw new ArgumentException("Removing nodes with more than 2 neighbors is not supported", "index");
            }
        }

        private void MergeInto(int x, int into)
        {
            if (!_isFixedTarget[into])
                throw new ArgumentException("Nodes can only be merged into fixed target nodes", "into");
            
            _searchSpace[into].MergeWith(_searchSpace[x], _distanceLookup.GetShortestPath(x, into));
            
            _edgeSet.Remove(x, into);
            var intoNeighbors = _edgeSet.NeighborsOf(into);
            foreach (var neighbor in _edgeSet.NeighborsOf(x))
            {
                var oldEdge = _edgeSet[x, neighbor];
                _edgeSet.Remove(oldEdge);
                if (!intoNeighbors.Contains(neighbor) || oldEdge.Weight < _edgeSet[into, neighbor].Weight)
                {
                    _edgeSet.Add(new GraphEdge(into, neighbor, oldEdge.Weight));
                }
            }
            
            _distanceLookup.MergeInto(x, into);
        }

        private void RemoveUnconnectedNodes()
        {
            for (var i = 0; i < _searchSpace.Count; i++)
            {
                if (!_edgeSet.HasNeighbors(i))
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
    }
}