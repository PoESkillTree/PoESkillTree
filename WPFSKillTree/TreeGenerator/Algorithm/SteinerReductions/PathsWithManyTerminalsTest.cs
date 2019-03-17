using System.Linq;
using PoESkillTree.TreeGenerator.Algorithm.Model;

namespace PoESkillTree.TreeGenerator.Algorithm.SteinerReductions
{
    /// <summary>
    /// A reduction tests that removes edges with a weight higher than the bottleneck Steiner distance
    /// between the edge nodes.
    /// </summary>
    /// <remarks>
    /// See <see cref="BottleneckSteinerDistanceCalculator"/> for more information about bottleneck
    /// Steiner distances.
    /// 
    /// Source of the original definition of the test:
    ///     C. W. Duin, A. Volgenant (1989): "An edge elimination test for the steiner problem in graphs"
    /// Source of the name "Paths with many Terminals":
    ///     T. Polzin (2003): "Algorithms for the Steiner Problem in Networks", p. 50
    /// </remarks>
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
            EdgeSet.Where(e => e.Weight > SMatrix[e.N1, e.N2]).ToList().ForEach(EdgeSet.Remove);

            return 0;
        }
    }
}