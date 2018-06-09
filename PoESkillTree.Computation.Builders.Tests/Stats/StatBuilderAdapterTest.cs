using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
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
        public void StatBuilderAdapterCombineWithReturnsComposite()
        {
            var statBuilder = Mock.Of<IStatBuilder>();
            var sut = new StatBuilderAdapter(statBuilder);

            var actual = sut.CombineWith(new LeafCoreStatBuilder("", null));

            Assert.IsInstanceOf<CompositeCoreStatBuilder>(actual);
        }

        [Test]
        public void StatBuilderAdapterBuildValueReturnsStatBuilderValueBuild()
        {
            var expected = Mock.Of<IValue>();
            var valueBuilder = Mock.Of<IValueBuilder>(b => b.Build(default) == expected);
            var statBuilder = Mock.Of<IStatBuilder>(b => b.Value == new ValueBuilder(valueBuilder));
            var sut = new StatBuilderAdapter(statBuilder);

            var actual = sut.BuildValue(default);

            Assert.AreEqual(expected, actual);
        }

        private static readonly ModifierSource ModifierSource = new ModifierSource.Global();
    }
}