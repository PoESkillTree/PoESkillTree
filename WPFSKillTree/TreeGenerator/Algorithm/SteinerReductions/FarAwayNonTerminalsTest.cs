using System.Linq;
using MoreLinq;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// A test that removes non-terminal that are not in reasonable vicinity.
    /// </summary>
    /// <remarks>
    /// "reasonable vicinity": The distance to the closest terminal is smaller
    /// than the longest edge of the MST over all terminals.
    /// 
    /// Non-terminals that are not in reasonable vicinity can safely be removed.
    /// </remarks>
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

            for (var i = 0; i < SearchSpaceSize; i++)
            {
                if (NodeStates.IsTarget(i) || NodeStates.IsRemoved(i)) continue;
                
                // Theoretically, this can be sped up by using Voronoi partitions. The Voronoi base of i is the
                // terminal with the smallest distance to i by definition, so only the distance to that terminal
                // has to be checked.
                if (NodeStates.FixedTargetNodeIndices.All(t => DistanceLookup[i, t] >= maxEdgeDistance))
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