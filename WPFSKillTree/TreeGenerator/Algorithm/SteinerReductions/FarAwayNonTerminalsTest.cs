using System.Linq;
using POESKillTree.TreeGenerator.Algorithm.Model;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public class FarAwayNonTerminalsTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Far away non Terminals"; }
        }

        public FarAwayNonTerminalsTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            if (NodeStates.FixedTargetNodeCount <= 1 || NodeStates.VariableTargetNodeCount > 0) return 0;

            var removedNodes = 0;

            var mst = new MinimalSpanningTree(NodeStates.FixedTargetNodeIndices.ToList(), DistanceLookup);
            mst.Span(StartNodeIndex);
            var maxEdgeDistance = mst.SpanningEdges.Max(e => DistanceLookup[e.Inside, e.Outside]);

            var voronoiPartition = new VoronoiPartition(DistanceLookup, NodeStates.FixedTargetNodeIndices, EdgeSet);

            for (var i = 0; i < SearchSpaceSize; i++)
            {
                if (NodeStates.IsTarget(i) || NodeStates.IsRemoved(i)) continue;

                if (DistanceLookup[i, voronoiPartition.Base(i)] >= maxEdgeDistance)
                {
                    EdgeSet.EdgesOf(i).ForEach(EdgeSet.Remove);
                    NodeStates.MarkNodeAsRemoved(i);
                    removedNodes++;
                }
            }

            return removedNodes;
        }
    }
}