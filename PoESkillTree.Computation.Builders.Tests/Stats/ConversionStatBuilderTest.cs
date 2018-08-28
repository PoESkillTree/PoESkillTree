using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Utils;
using static PoESkillTree.Computation.Builders.Tests.Stats.StatBuilderHelper;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    // ConversionStatBuilder is also tested from StatBuilderTest.
    [TestFixture]
    public class ConversionStatBuilderTest
    {
        [Test]
        public void BuildThrowsIfSourceAndTargetEntityDoNotMatch()
        {
            var source = CreateStatBuilder("s", Entity.Character);
            var target = CreateStatBuilder("t", Entity.Enemy);
            var sut = CreateSut(source, target);

            AssertBuildThrows(sut);
        }

        [Test]
        public void BuildGroupsStatsByEntity()
        {
            var expected = new[]
                { new Stat("s.ConvertTo(t)", Entity.Character), new Stat("s.ConvertTo(t)", Entity.Enemy) };
            var source = CreateStatBuilder("s", Entity.Character, Entity.Enemy);
            var target = CreateStatBuilder("t", Entity.Enemy, Entity.Character);
            var sut = CreateSut(source, target);

            var actual = sut.Build(default).Single().Stats;

            Assert.AreEqual(6, actual.Count);
            CollectionAssert.IsSubsetOf(expected, actual);
        }

        [Test]
        public void BuildChainsValueConverters()
        {
            var valueBuilders = Helper.MockMany<IValueBuilder>();
            var source = MockStatBuilder(valueConverter: _ => valueBuilders[1]);
            var target = MockStatBuilder(valueConverter: _ => valueBuilders[2]);
            var sut = CreateSut(source, target);

            var valueConverter = sut.Build(default).Single().ValueConverter;
            var actual = valueConverter(valueBuilders[0]);

            Assert.AreEqual(valueBuilders[2], actual);
        }

        [Test]
        public void BuildThrowsIfSourceModifierSourceIsNotOriginal()
        {
            var source = MockStatBuilder(new ModifierSource.Local.Given());
            var target = MockStatBuilder();
            var sut = CreateSut(source, target);

            AssertBuildThrows(sut);
        }

        [Test]
        public void BuildThrowsIfTargetModifierSourceIsNotOriginal()
        {
            var source = MockStatBuilder();
            var target = MockStatBuilder(new ModifierSource.Local.Given());
            var sut = CreateSut(source, target);

            AssertBuildThrows(sut);
        }

        [Test]
        public void ResolveResolvesSourceAndTarget()
        {
            var source = new Mock<ICoreStatBuilder>();
            var target = new Mock<ICoreStatBuilder>();
            var sut = CreateSut(source.Object, target.Object);

            sut.Resolve(null);

            source.Verify(b => b.Resolve(null));
            target.Verify(b => b.Resolve(null));
        }

        [Test]
        public void WithEntityAppliesToSource()
        {
            var entityBuilder = Mock.Of<IEntityBuilder>();
            var source = new Mock<ICoreStatBuilder>();
            var sut = CreateSut(source.Object, MockStatBuilder());

            sut.WithEntity(entityBuilder);

            source.Verify(b => b.WithEntity(entityBuilder));
        }

        [Test]
        public void BuildZipsSourceAndTargetResults()
        {
            var source = MockStatBuilder(CreateStatBuilderResult(), CreateStatBuilderResult());
            var target = MockStatBuilder(CreateStatBuilderResult(), CreateStatBuilderResult());
            var sut = CreateSut(source, target);

            var actual = sut.Build(default);

            Assert.That(actual, Has.Exactly(2).Items);
        }

        [Test]
        public void BuildThrowsIfSourceAndTargetHaveDifferentResultCounts()
        {
            var source = MockStatBuilder(CreateStatBuilderResult(), CreateStatBuilderResult());
            var target = MockStatBuilder(CreateStatBuilderResult());
            var sut = CreateSut(source, target);

            AssertBuildThrows(sut);
        }

        private static ConversionStatBuilder CreateSut(ICoreStatBuilder source, ICoreStatBuilder target) =>
            new ConversionStatBuilder((s, ts) => new StatFactory().ConvertTo(s, ts), source, target);

        private static void AssertBuildThrows(ICoreStatBuilder sut)
        {
            Assert.Throws<ParseException>(() => sut.Build(default).Consume());
        }

        private static ICoreStatBuilder MockStatBuilder(
            ModifierSource modifierSource = null, ValueConverter valueConverter = null) =>
            MockStatBuilder(CreateStatBuilderResult(modifierSource, valueConverter));

        private static ICoreStatBuilder MockStatBuilder(params StatBuilderResult[] results) =>
            Mock.Of<ICoreStatBuilder>(b => b.Build(default) == results);

        private static StatBuilderResult CreateStatBuilderResult(
            ModifierSource modifierSource = null, ValueConverter valueConverter = null, params IStat[] stats) =>
            new StatBuilderResult(stats, modifierSource, valueConverter ?? Funcs.Identity);
    }
}