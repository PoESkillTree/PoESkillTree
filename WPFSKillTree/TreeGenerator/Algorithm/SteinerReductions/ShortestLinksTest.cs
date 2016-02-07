using System;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
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
            var voronoiPartition = new VoronoiPartition(DistanceLookup, NodeStates.FixedTargetNodes.Select(n => n.DistancesIndex),
                EdgeSet);

            for (var i = 0; i < SearchSpaceSize; i++)
            {
                if (!NodeStates.IsFixedTarget(i) || terminalVisited[i]) continue;

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
                    DistanceLookup[iNonTerminal, i] + shortestEdge.Weight +
                    DistanceLookup[otherNonTerminal, otherTerminal])
                {
                    var into = NodeStates.IsFixedTarget(iNonTerminal)
                        ? iNonTerminal
                        : (NodeStates.IsTarget(otherNonTerminal) ? otherNonTerminal : iNonTerminal);
                    var x = into == iNonTerminal ? otherNonTerminal : iNonTerminal;
                    NodeStates.SetFixedTarget(into);
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
    }
}