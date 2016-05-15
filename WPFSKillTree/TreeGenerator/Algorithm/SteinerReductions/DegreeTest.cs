using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// A test that uses 4 fast degree based reductions.
    /// </summary>
    /// <remarks>
    /// 1. non-terminals of degree 1 can be removed.
    /// 2. non-terminals of degree 2 can be removed after the edges to their
    ///    two neighbors are combined into one edge.
    /// 3. non-terminals adjacent to terminals of degree 1 must be taken
    ///    and can be merged into the terminal.
    /// 4. An edge connecting two terminals that is the shortest edge of
    ///    one of those terminals can be in an optimal solution. The terminals
    ///    can be combined into one.
    /// 
    /// Source:
    ///     T. Koch, A. Martin (1998): "Solving Steiner Tree Problems in Graphs to Optimality"
    ///     (test was first published by J. E. Beasley in 1984)
    /// </remarks>
    public class DegreeTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Degree"; }
        }

        public DegreeTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            var removedNodes = 0;

            var untested = new HashSet<int>(Enumerable.Range(0, SearchSpaceSize));
            while (untested.Any())
            {
                var i = untested.First();
                untested.Remove(i);

                var neighbors = EdgeSet.NeighborsOf(i);

                if (!NodeStates.IsTarget(i))
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
                else if (NodeStates.IsFixedTarget(i))
                {
                    if (neighbors.Count == 1)
                    {
                        var other = neighbors[0];
                        // Fixed target nodes with one neighbor can be merged with their neighbor since
                        // it must always be taken.
                        // If the neighbor is no target and of degree 1 or 2, it will be removed by the tests above,
                        // after which this node will be processed again.
                        if (EdgeSet.NeighborsOf(other).Count <= 2 && !NodeStates.IsTarget(other)) continue;
                        // If this node is the only fixed target, the neighbor must not be merged because
                        //  if this is the only target, the neighbor and the rest of the tree will not be in the optimal solution,
                        //  if there are only variable target nodes, the neighbor will not be in the optimal solution if the variable nodes are not taken.
                        if (NodeStates.FixedTargetNodeCount == 1) continue;
                        untested.Add(i);
                        untested.Remove(other);
                        untested.UnionWith(MergeInto(other, i));
                        removedNodes++;
                    }
                    else if (neighbors.Count > 1)
                    {
                        // Edges from one target node to another that are of minimum cost among the edges of
                        // one of the nodes can be in any optimal solution. Therefore both target nodes can be merged.
                        var minimumEdgeCost = neighbors.Min(other => DistanceLookup[i, other]);
                        var minimumTargetNeighbors =
                            neighbors.Where(other => DistanceLookup[i, other] == minimumEdgeCost && NodeStates.IsFixedTarget(other));
                        foreach (var other in minimumTargetNeighbors)
                        {
                            untested.Add(i);
                            untested.Remove(other);
                            untested.UnionWith(MergeInto(other, i));
                            removedNodes++;
                        }
                    }
                }
            }

            return removedNodes;
        }

        private void RemoveNode(int index)
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
                    // Merge the two incident edges.
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
    }
}