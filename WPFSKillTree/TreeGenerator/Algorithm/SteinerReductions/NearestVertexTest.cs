using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.TreeGenerator.Algorithm.Model;

namespace PoESkillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// A reduction test that merges nearest direct neighbors of terminals into them.
    /// </summary>
    /// <remarks>
    /// See the inline documentation for a more detailed explanation.
    /// 
    /// Source of the test:
    ///     T. Polzin (2003): "Algorithms for the Steiner Problem in Networks", p. 54
    ///     (test was first published by J. E. Beasley in 1984)
    /// </remarks>
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
            // The test only makes sense with at least 2 terminals.
            if (NodeStates.FixedTargetNodeCount <= 1) return 0;

            var removedNodes = 0;
            var untested = new HashSet<int>(NodeStates.FixedTargetNodeIndices);
            // For each terminal z with degree of at least 2
            while (untested.Any())
            {
                var z = untested.First();
                untested.Remove(z);

                var neighbors = EdgeSet.NeighborsOf(z);
                if (neighbors.Count < 2) continue;

                // Determine the shortest and second shortest edge incident to z.
                // For the second shortest, only the weight is of interest.
                var tuple = ShortestTwoEdgesOf(EdgeSet.EdgesOf(z));
                var shortest = tuple.Item1;
                var secondShortestWeight = tuple.Item2;
                // v is the node which is connected to z via the shortest edge.
                var v = shortest.N1 == z ? shortest.N2 : shortest.N1;

                // The shortest edge belongs to at least one Steiner minimal tree, if
                // secondShortestWeight >= shortest.Weight + distance(v, y) for any terminal y, y != z
                var canBeContracted = NodeStates.FixedTargetNodeIndices
                        .Where(y => z != y)
                        .Any(y => secondShortestWeight >= shortest.Weight + DistanceLookup[v, y]);
                // If such a y exists, we can merge v into z.
                if (canBeContracted)
                {
                    // z was changed and can be tested again.
                    untested.Add(z);
                    // v no longer exists and as such must not be tested.
                    untested.Remove(v);
                    MergeInto(v, z);
                    removedNodes++;
                }
            }
            return removedNodes;
        }

        /// <summary>
        /// Returns the two edges of the parameter which have the lowest weights.
        /// </summary>
        /// <returns>A tuple of the edge with the lowest weight and the weight of the second shortest edge.</returns>
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
    }
}