using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public class NearestVertexTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Nearest Vertex"; }
        }

        public NearestVertexTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            if (NodeStates.FixedTargetNodeCount == 1) return 0;

            var removedNodes = 0;
            var untested = new HashSet<int>(NodeStates.FixedTargetNodes.Select(n => n.DistancesIndex));
            // For each terminal zi with degree of at least 2
            while (untested.Any())
            {
                var zi = untested.First();
                untested.Remove(zi);

                var neighbors = EdgeSet.NeighborsOf(zi);
                if (neighbors.Count < 2) continue;

                // Let (zi, v1) and (zi, v2) be the shortest and second shortest edges incident to zi.
                var tuple = ShortestTwoEdgesOf(EdgeSet.EdgesOf(zi));
                var shortest = tuple.Item1;
                var secondShortestWeight = tuple.Item2;

                var v1 = shortest.N1 == zi ? shortest.N2 : shortest.N1;

                // (zi, v1) belongs to at least one Steiner minimal tree, if there is a terminal zj, zi != zj
                // with: c(zi, v2) >= c(zi, v1) + d(v1, zj)
                var canBeContracted =
                    NodeStates.FixedTargetNodes.Select(n => n.DistancesIndex)
                        .Where(zj => zi != zj)
                        .Any(zj => secondShortestWeight >= shortest.Weight + DistanceLookup[v1, zj]);
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
    }
}