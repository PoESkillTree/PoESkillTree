using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm.SteinerReductions
{
    public class PathsWithManyTerminalsTest : SteinerReduction
    {
        protected override string TestId
        {
            get { return "Paths with many Terminals"; }
        }

        public PathsWithManyTerminalsTest(INodeStates nodeStates, IData data) : base(nodeStates, data)
        {
        }

        protected override int ExecuteTest()
        {
            EdgeSet.Where(e => e.Weight > SMatrix[e.N1, e.N2])
                .ToList().ForEach(EdgeSet.Remove);

            return 0;
        }
    }
}