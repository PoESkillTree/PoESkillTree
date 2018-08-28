using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class MetaStatBuildersTest
    {
        [TestCase(Pool.Life, 42)]
        [TestCase(Pool.Life, 0)]
        [TestCase(Pool.Mana, 3)]
        public void RegenTargetPoolValueCalculatesCorrectly(Pool targetPool, double expected)
        {
            var statFactory = new StatFactory();
            var targetPoolStat = statFactory.RegenTargetPool(default, Pool.Life);
            var targetPoolValueStat = statFactory.FromIdentity(targetPool.ToString(), default, typeof(int));
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(targetPoolStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) (int) targetPool &&
                c.GetValue(targetPoolValueStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) expected);
            var sut = new MetaStatBuilders(statFactory);

            var valueBuilder = sut.RegenTargetPoolValue(Pool.Life);
            var actual = valueBuilder.Build().Calculate(context);

            Assert.AreEqual(expected, actual.Single());
        }
    }
}