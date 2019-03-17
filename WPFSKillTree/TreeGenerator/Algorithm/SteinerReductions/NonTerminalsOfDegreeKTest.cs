using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.TreeGenerator.Algorithm.Model;

namespace PoESkillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// A reduction test that removes non-terminals and replaces them by merging each pair of incident edges.
    /// </summary>
    /// <remarks>
    /// A non-terminal v can be replaced by merging each pair of incident edges, if edgeSum >= mstSum
    /// for all subsets of adjacent nodes of at least size 3, with:
    /// - edgeSum being the sum of the weights of the edges between v and the nodes in the subset
    /// - mst being the MST over the nodes in the subset with edges between all nodes with the bottleneck
    ///   Steiner distances as weights.
    /// - mstSum being the sum of the weights of the edges of mst
    /// Should become more clear by reading the inline documentation.
    /// 
    /// Source of the test:
    ///     T. Polzin (2003): "Algorithms for the Steiner Problem in Networks", p. 54
    ///     (test was first published by C. W. Duin and T. Volgenant in 1989)
    /// </remarks>
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
                // This test only checks non-terminals.
                if (NodeStates.IsTarget(i)) continue;

                // Nodes with less than 3 neighbors are covered by DegreeTest
                // Nodes are limited to 7 neighbors because this test is exponential in the neighbor count.
                var neighbors = EdgeSet.NeighborsOf(i);
                if (neighbors.Count < 3 || neighbors.Count > 7) continue;
                
                // Cache the edges. They might be removed from EdgeSet when we need them.
                foreach (var neighbor in neighbors)
                {
                    edges[neighbor] = EdgeSet[i, neighbor];
                }

                // Check whether each subset satisfies the condition.
                var canBeRemoved = true;
                foreach (var subset in GetAllSubsets(neighbors))
                {
                    // Only subsets of at least size 3 are relevant.
                    if (subset.Count < 3) continue;

                    // Sum up the weights of all edges between the nodes of the subsets and i.
                    var edgeSum = subset.Sum(j => edges[j].Weight);
                    // Build the MST of the fully connected graph of the nodes in the subset with the bottleneck
                    // Steiner distances as edge weights.
                    var mst = new MinimalSpanningTree(subset, SMatrix);
                    mst.Span(subset[0]);
                    // Sum up the edge weights of the MST.
                    var mstSum = mst.SpanningEdges.Sum(e => DistanceLookup[e.Inside, e.Outside]);
                    // The condition is only satisfied if edgeSum >= mstSum.
                    if (edgeSum < mstSum)
                    {
                        canBeRemoved = false;
                        break;
                    }
                }

                if (!canBeRemoved) continue;

                // Remove i and replace its edges.
                foreach (var neighbor in neighbors)
                {
                    // Remove the old edges between i and its neighbors.
                    var edge = edges[neighbor];
                    EdgeSet.Remove(edge);
                    // For each pair of neighbors create a new edge.
                    foreach (var neighbor2 in neighbors)
                    {
                        if (neighbor >= neighbor2) continue;
                        // The weight of the new edge between the two neighbors is the sum of their edge weights to i.
                        var edge2 = edges[neighbor2];
                        var newEdgeWeight = edge.Weight + edge2.Weight;
                        // Only add this edge if it wouldn't be removed by the Paths with many terminals test
                        // and if it is of optimal length.
                        if (newEdgeWeight <= SMatrix[neighbor, neighbor2]
                            && newEdgeWeight <= DistanceLookup[neighbor, neighbor2])
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

        /// <summary>
        /// Returns an enumeration that contains all subsets of the parameter.
        /// </summary>
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