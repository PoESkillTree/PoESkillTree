using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
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
            var dependentNodes = new Dictionary<int, List<int>>();
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
                        if (EdgeSet.NeighborsOf(other).Count > 2 || NodeStates.IsTarget(other))
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

                List<int> dependent;
                if (dependentNodes.TryGetValue(i, out dependent))
                {
                    untested.UnionWith(dependent);
                }
            }

            return removedNodes;
        }
    }
}