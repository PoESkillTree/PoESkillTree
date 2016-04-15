using System;
using System.Diagnostics;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    // T. Polzin (2003): "Algorithms for the Steiner Problem in Networks", p. 55
    public class ShortestLinksTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Shortest Links"; }
        }

        public ShortestLinksTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            if (NodeStates.FixedTargetNodeCount == 1) return 0;

            var removedNodes = 0;

            var terminalVisited = new bool[SearchSpaceSize];
            var voronoiPartition = new VoronoiPartition(DistanceLookup, NodeStates.FixedTargetNodeIndices,
                EdgeSet);

            for (var i = 0; i < SearchSpaceSize; i++)
            {
                if (!NodeStates.IsFixedTarget(i) || terminalVisited[i]) continue;

                var links = voronoiPartition.Links(i);
                // ShortestTwoEdgesOf was moved from SteinerReduction to NearestVertexTest
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
                    DistanceLookup[iNonTerminal, i] + shortestEdge.Weight +
                    DistanceLookup[otherNonTerminal, otherTerminal])
                {
                    var into = NodeStates.IsFixedTarget(iNonTerminal)
                        ? iNonTerminal
                        : (NodeStates.IsTarget(otherNonTerminal) ? otherNonTerminal : iNonTerminal);
                    var x = into == iNonTerminal ? otherNonTerminal : iNonTerminal;

                    // Requiring one of the nodes to already be a terminal.
                    // Simplifies the code without any practical disadvantage (it very rarely happens that both are non-terminals in our scenarios).
                    if (!NodeStates.IsFixedTarget(into)) continue;
                    Debug.Assert(iNonTerminal == i || otherNonTerminal == otherTerminal, "Voronoi-base of a terminal must be itself.");

                    MergeInto(x, into);
                    terminalVisited[i] = true;
                    terminalVisited[otherTerminal] = true;
                    removedNodes++;
                }
            }

            return removedNodes;
        }
    }
}