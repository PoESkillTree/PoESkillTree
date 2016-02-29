using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public abstract class SteinerReduction
    {
        private int _iteration;

        private readonly IData _data;

        protected GraphEdgeSet EdgeSet
        {
            get { return _data.EdgeSet; }
        }

        protected int SearchSpaceSize
        {
            get { return _data.DistanceLookup.CacheSize; }
        }

        protected IDistanceLookup DistanceLookup
        {
            get { return _data.DistanceLookup; }
        }

        protected IDistanceLookup SMatrix
        {
            get { return _data.SMatrix; }
        }

        protected GraphNode StartNode
        {
            get { return _data.StartNode; }
            private set { _data.StartNode = value; }
        }

        protected abstract string TestId { get; }

        protected INodeStates NodeStates { get; private set; }

        protected abstract int ExecuteTest();

        public bool IsEnabled { get; set; }

        protected SteinerReduction(INodeStates nodeStates, IData data)
        {
            _data = data;
            NodeStates = nodeStates;
            IsEnabled = true;
        }

        public bool RunTest(ref int edgeElims, ref int nodeElims)
        {
            if (!IsEnabled)
            {
                return false;
            }
            var edgeCountBefore = EdgeSet.Count;
            var removedNodes = ExecuteTest();
            Debug.WriteLine("{0} Test #{1}:", TestId, ++_iteration);
            Debug.WriteLine("   removed nodes: " + removedNodes);
            Debug.WriteLine("   removed edges: " + (edgeCountBefore - EdgeSet.Count));
            edgeElims += edgeCountBefore - EdgeSet.Count;
            nodeElims += removedNodes;
            return edgeCountBefore - EdgeSet.Count > 0;
        }

        protected void RemoveNode(int index)
        {
            if (NodeStates.IsTarget(index))
                throw new ArgumentException("Target nodes can't be removed", "index");

            var neighbors = EdgeSet.NeighborsOf(index);
            switch (neighbors.Count)
            {
                case 0:
                    break;
                case 1:
                    EdgeSet.Remove(index, neighbors[0]);
                    break;
                case 2:
                    var left = neighbors[0];
                    var right = neighbors[1];
                    var newWeight = EdgeSet[index, left].Weight + EdgeSet[index, right].Weight;
                    EdgeSet.Remove(index, left);
                    EdgeSet.Remove(index, right);
                    if (newWeight <= DistanceLookup[left, right])
                    {
                        EdgeSet.Add(left, right, newWeight);
                    }
                    break;
                default:
                    throw new ArgumentException("Removing nodes with more than 2 neighbors is not supported", "index");
            }

            NodeStates.MarkNodeAsRemoved(index);
        }

        protected IEnumerable<int> MergeInto(int x, int into)
        {
            if (!NodeStates.IsFixedTarget(into))
                throw new ArgumentException("Nodes can only be merged into fixed target nodes", "into");

            _data.DistanceLookup.IndexToNode(into).MergeWith(_data.DistanceLookup.IndexToNode(x), _data.DistanceLookup.GetShortestPath(x, into));
            _data.DistanceLookup.MergeInto(x, into);

            EdgeSet.Remove(x, into);
            var intoNeighbors = EdgeSet.NeighborsOf(into);
            var xNeighbors = EdgeSet.NeighborsOf(x);
            var neighbors = intoNeighbors.Union(xNeighbors);
            foreach (var neighbor in xNeighbors)
            {
                EdgeSet.Remove(x, neighbor);
            }
            foreach (var neighbor in neighbors)
            {
                EdgeSet.Add(into, neighbor, _data.DistanceLookup[into, neighbor]);
            }

            if (StartNode.DistancesIndex == x)
            {
                StartNode = _data.DistanceLookup.IndexToNode(into);
            }

            NodeStates.MarkNodeAsRemoved(x);

            return xNeighbors;
        }
        
        protected static IEnumerable<List<int>> GetAllSubsets(IReadOnlyList<int> of)
        {
            var subsets = new List<List<int>>((int)Math.Pow(2, of.Count));
            for (var i = 1; i < of.Count; i++)
            {
                subsets.Add(new List<int>(new[] { of[i - 1] }));
                var i1 = i;
                var newSubsets = subsets.Select(subset => subset.Concat(new[] { of[i1] }).ToList()).ToList();
                subsets.AddRange(newSubsets);
            }
            subsets.Add(new List<int>(new[] { of.Last() }));
            return subsets;
        }

        protected static Tuple<GraphEdge, uint> ShortestTwoEdgesOf(IReadOnlyList<GraphEdge> edges)
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
        
    }
}