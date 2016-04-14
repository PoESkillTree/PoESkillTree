using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public class NonTerminalsOfDegreeKTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Non Terminals of degree k"; }
        }

        public NonTerminalsOfDegreeKTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            var edges = new GraphEdge[SearchSpaceSize];
            var removedNodes = 0;
            for (var i = 0; i < SearchSpaceSize; i++)
            {
                var neighbors = EdgeSet.NeighborsOf(i);
                if (neighbors.Count < 3 || neighbors.Count > 6 || NodeStates.IsTarget(i)) continue;
                
                foreach (var neighbor in neighbors)
                {
                    edges[neighbor] = EdgeSet[i, neighbor];
                }

                var canBeRemoved = true;
                foreach (var subset in GetAllSubsets(neighbors))
                {
                    if (subset.Count < 3) continue;

                    var edgeSum = subset.Sum(j => edges[j].Weight);
                    var mst = new MinimalSpanningTree(subset, SMatrix);
                    mst.Span(subset[0]);
                    var mstEdgeSum = mst.SpanningEdges.Sum(e => e.Priority);
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
                    EdgeSet.Remove(edge);
                    foreach (var neighbor2 in neighbors)
                    {
                        if (neighbor >= neighbor2) continue;
                        var edge2 = edges[neighbor2];
                        var newEdgeWeight = edge.Weight + edge2.Weight;
                        // Implicit path with many terminals test: don't add edges that would be removed by it.
                        if (newEdgeWeight <= SMatrix[neighbor, neighbor2])
                        {
                            EdgeSet.Add(neighbor, neighbor2, newEdgeWeight);
                        }
                    }
                }

                NodeStates.MarkNodeAsRemoved(i);
                removedNodes++;
            }
            return removedNodes;
        }

        private static IEnumerable<List<int>> GetAllSubsets(IReadOnlyList<int> of)
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
    }
}