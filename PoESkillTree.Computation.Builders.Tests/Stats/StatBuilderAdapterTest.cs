using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    // Most of the tests for StatBuilderAdapter are in StatBuilderTest. This only tests things not tested through
    // StatBuilder.
    [TestFixture]
    public class StatBuilderAdapterTest
    {
        [Test]
        public void BuildValueReturnsStatBuilderValueBuild()
        {
            var expected = Mock.Of<IValue>();
            var valueBuilder = Mock.Of<IValueBuilder>(b => b.Build(default) == expected);
            var statBuilder = Mock.Of<IStatBuilder>(b => b.Value == new ValueBuilder(valueBuilder));
            var sut = new StatBuilderAdapter(statBuilder);

            var actual = sut.BuildValue(default);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ResolveResolvesCondition()
        {
            var statBuilder = Mock.Of<IStatBuilder>();
            var conditionBuilder = new Mock<IConditionBuilder>();
            var sut = new StatBuilderAdapter(statBuilder, conditionBuilder.Object);

            sut.Resolve(null);

            conditionBuilder.Verify(b => b.Resolve(null));
        }
    }
}