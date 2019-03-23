using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Builders.Actions
{
    [TestFixture]
    public class ActionBuilderTest
    {
        [TestCase(2)]
        [TestCase(3)]
        public void InPastXSecondsBuildsToCorrectResult(int seconds)
        {
            var lastOccurence = 3;
            var expected = lastOccurence <= seconds;
            var secondsValue = new ValueBuilderImpl(seconds);
            var lastOccurenceStat = new Stat("test.LastOccurrence");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(lastOccurence));
            var sut = CreateSut();

            var result = sut.InPastXSeconds(secondsValue).Build();

            Assert.IsFalse(result.HasStatConverter);
            Assert.IsTrue(result.HasValue);
            var actual = result.Value.Calculate(context);
            Assert.AreEqual(expected, actual.IsTrue());
        }

        [Test]
        public void InPastXSecondsBuildsToCorrectResultIfLastOccurenceIsNull()
        {
            var secondsValue = new ValueBuilderImpl(5);
            var lastOccurenceStat = new Stat("test.LastOccurrence");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == null);
            var sut = CreateSut();

            var result = sut.InPastXSeconds(secondsValue).Build();
            var actual = result.Value.Calculate(context);

            Assert.IsFalse(actual.IsTrue());
        }

        [TestCase(2)]
        [TestCase(3)]
        public void CountRecentlyBuildsToCorrectResult(int count)
        {
            var expected = (NodeValue?) count;
            var countStat = new Stat("test.RecentOccurrences");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(countStat, NodeType.Total, PathDefinition.MainPath) == expected);
            var sut = CreateSut();

            var result = sut.CountRecently.Build();
            var actual = result.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RecentlyBuildsToCorrectResultIfHasRecentOccurrences()
        {
            var lastOccurenceStat = new Stat("test.LastOccurrence");
            var countStat = new Stat("test.RecentOccurrences");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == null &&
                c.GetValue(countStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(2));
            var sut = CreateSut();

            var result = sut.Recently.Build();
            var actual = result.Value.Calculate(context);

            Assert.IsTrue(actual.IsTrue());
        }

        [Test]
        public void ByRecentlyBuildsToCorrectResult()
        {
            var entityBuilder = new EntityBuilder(Entity.Enemy);
            var lastOccurenceStat = new Stat("test.LastOccurrence", Entity.Enemy);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1));
            var sut = CreateSut();

            var result = sut.By(entityBuilder).Recently.Build();
            var actual = result.Value.Calculate(context);

            Assert.IsTrue(actual.IsTrue());
        }

        [Test]
        public void ResolveRecentlyBuildsToCorrectResult()
        {
            var identityBuilder = CoreBuilder.Create("test");
            var unresolvedIdentityBuilder = Mock.Of<ICoreBuilder<string>>(b => b.Resolve(null) == identityBuilder);
            var lastOccurenceStat = new Stat("test.LastOccurrence");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1));
            var sut = CreateSut(unresolvedIdentityBuilder);

            var result = sut.Resolve(null).Recently.Build();
            var actual = result.Value.Calculate(context);

            Assert.IsTrue(actual.IsTrue());
        }

        [Test]
        public void RecentlyResolveBuildsToCorrectResult()
        {
            var identityBuilder = CoreBuilder.Create("test");
            var unresolvedIdentityBuilder = Mock.Of<ICoreBuilder<string>>(b => b.Resolve(null) == identityBuilder);
            var lastOccurenceStat = new Stat("test.LastOccurrence");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(lastOccurenceStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1));
            var sut = CreateSut(unresolvedIdentityBuilder);

            var result = sut.Recently.Resolve(null).Build();
            var actual = result.Value.Calculate(context);

            Assert.IsTrue(actual.IsTrue());
        }

        [Test]
        public void OnBuildsToCorrectResult()
        {
            var sut = CreateSut();

            var result = sut.On.Build();
            var statBuilder = result.StatConverter(InputStat);
            var stat = statBuilder.BuildToSingleStat();

            Assert.IsTrue(result.HasStatConverter);
            Assert.IsFalse(result.HasValue);
            Assert.AreEqual("stat.On(test).By(Character)", stat.Identity);
            Assert.AreEqual(new ExplicitRegistrationType.GainOnAction(new Stat("stat"), "test", Entity.Character),
                stat.ExplicitRegistrationType);
        }

        [Test]
        public void ByOnBuildsToCorrectResult()
        {
            var entityBuilder = new EntityBuilder(Entity.Enemy, Entity.Totem);
            var sut = CreateSut();

            var result = sut.By(entityBuilder).On.Build();
            var statBuilder = result.StatConverter(InputStat);
            IReadOnlyList<IStat> stats = statBuilder.BuildToSingleResult().Stats;

            Assert.That(stats, Has.Exactly(2).Items);
            Assert.AreEqual("Character.stat.On(test).By(Enemy)", stats[0].ToString());
            Assert.AreEqual("Character.stat.On(test).By(Totem)", stats[1].ToString());
        }

        [Test]
        public void OnResolvesIdentity()
        {
            var identityBuilder = CoreBuilder.Create("test");
            var unresolvedBuilder = Mock.Of<ICoreBuilder<string>>(b => b.Resolve(null) == identityBuilder);
            var sut = CreateSut(unresolvedBuilder);

            var result = sut.On.Resolve(null).Build();
            var stat = result.StatConverter(InputStat).BuildToSingleStat();

            Assert.AreEqual("stat.On(test).By(Character)", stat.Identity);
        }

        private static ActionBuilder CreateSut(ICoreBuilder<string> identity = null, IEntityBuilder entity = null) =>
            new ActionBuilder(new StatFactory(), identity ?? CoreBuilder.Create("test"),
                entity ?? new ModifierSourceEntityBuilder());

        private static IStatBuilder InputStat => StatBuilderUtils.FromIdentity(new StatFactory(), "stat", null);
    }
}