using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class StatBuilderTest
    {
        [Test]
        public void BuildReturnsPassedModifierSource()
        {
            var sut = CreateSut();
            var expected = new ModifierSource.Global();

            var (_, actual, _) = sut.Build(expected, Entity.Character);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildReturnsIdentityValueConverter()
        {
            var sut = CreateSut();
            ValueConverter expected = Funcs.Identity;

            var (_, _, actual) = sut.Build(new ModifierSource.Global(), Entity.Character);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character, Entity.Totem, Entity.Minion)]
        public void BuildReturnsOneStatPerEntity(params Entity[] entities)
        {
            var sut = CreateSut(entities);

            var (actual, _, _) = sut.Build(new ModifierSource.Global(), Entity.Character);

            Assert.AreEqual(entities, actual.Select(s => s.Entity));
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character)]
        public void BuildReturnsStatForPassedEntityIfEntityBuilderBuildsToEmpty(Entity expected)
        {
            var sut = CreateSut();

            var actual = BuildToSingleStat(sut, expected);

            Assert.AreEqual(expected, actual.Entity);
        }

        [TestCase(true, typeof(int), 0)]
        [TestCase(false, typeof(double), 3)]
        public void BuildReturnsStatWithPropertiesCopiedFromSelf(
            bool isRegisteredExplicitly, Type dataType, int behaviorCount)
        {
            var behaviors = new List<Behavior>();
            var sut = new StatBuilder("", new EntityBuilder(), isRegisteredExplicitly, dataType,
                behaviors);

            var actual = BuildToSingleStat(sut);

            Assert.AreEqual(isRegisteredExplicitly, actual.IsRegisteredExplicitly);
            Assert.AreEqual(dataType, actual.DataType);
            Assert.AreSame(behaviors, actual.Behaviors);
        }

        [TestCase("1234")]
        [TestCase("test")]
        public void StatToStringReturnsPassedIdentity(string identity)
        {
            var sut = CreateSut(identity);

            var actual = BuildToSingleStat(sut).ToString();

            Assert.AreEqual(identity, actual);
        }

        [TestCase("1", Entity.Enemy)]
        [TestCase("2", Entity.Enemy)]
        [TestCase("2", Entity.Character)]
        public void StatEqualsComparesIdentityAndEntity(string identity, Entity entity)
        {
            var comparedStat = Mock.Of<IStat>(s => s.ToString() == identity && s.Entity == entity);
            var statBuilder = CreateSut("1", Entity.Enemy);
            var sut = BuildToSingleStat(statBuilder);
            var expected = identity == "1" && entity == Entity.Enemy;

            var actual = sut.Equals(comparedStat);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, Entity.Enemy)]
        [TestCase(2, Entity.Character)]
        public void ValueBuildsToCorrectValue(double expected, Entity entity)
        {
            var sut = CreateSut(entity);
            var stat = BuildToSingleStat(sut);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) expected);

            var value = sut.Value.Build();
            var actual = value.Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [TestCase(Entity.Enemy)]
        public void ValueBuildUsesPassedEntity(Entity entity)
        {
            var expected = new NodeValue();
            var sut = CreateSut();
            var stat = BuildToSingleStat(sut, entity);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Total, PathDefinition.MainPath) == expected);

            var value = sut.Value.Build(entity);
            var actual = value.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueBuildThrowsWithMultipleEntities()
        {
            var sut = CreateSut(Entity.Character, Entity.Enemy);

            Assert.Throws<InvalidOperationException>(() => sut.Value.Build());
        }

        [Test]
        public void ValueBuildResolvesEntityBuilder()
        {
            var resolvedEntityBuilder = new EntityBuilder(default(Entity));
            var entityBuilder = Mock.Of<IEntityBuilder>(b => b.Resolve(null) == resolvedEntityBuilder);
            var sut = CreateSut("", entityBuilder);
            var resolvedStat = BuildToSingleStat(CreateSut("", resolvedEntityBuilder));
            var expected = new NodeValue(2);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(resolvedStat, NodeType.Total, PathDefinition.MainPath) == expected);

            var value = sut.Value.Resolve(null).Build();
            var actual = value.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        [TestCase("test")]
        public void MinimumBuildIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Minimum", default);
            var sut = CreateSut(identity);

            var actual = BuildToSingleStat(sut.Minimum);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void MaximumBuildIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Maximum", default);
            var sut = CreateSut(identity);

            var actual = BuildToSingleStat(sut.Maximum);

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void BuildMinimumIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Minimum", default);
            var sut = BuildToSingleStat(CreateSut(identity));

            var actual = sut.Minimum;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void BuildMaximumIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Maximum", default);
            var sut = BuildToSingleStat(CreateSut(identity));

            var actual = sut.Maximum;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MaximumMaximumBuildsToNull()
        {
            var sut = CreateSut().Maximum;

            var actual = sut.Maximum;

            Assert.IsNull(BuildToSingleStat(actual));
        }

        [Test]
        public void MinimumMinimumBuildsToNull()
        {
            var sut = CreateSut().Minimum;

            var actual = sut.Minimum;

            Assert.IsNull(BuildToSingleStat(actual));
        }

        [Test]
        public void ForBuildsUsingPassedEntity()
        {
            var expected = Entity.Enemy;
            var sut = CreateSut();

            var actual = BuildToSingleStat(sut.For(new EntityBuilder(expected))).Entity;

            Assert.AreEqual(expected, actual);
        }

        private static IStat BuildToSingleStat(IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            var (stats, _, _) = statBuilder.Build(new ModifierSource.Global(), entity);
            Assert.That(stats, Has.One.Items);
            return stats.Single();
        }

        private static StatBuilder CreateSut(params Entity[] entities) => CreateSut("", entities);

        private static StatBuilder CreateSut(string identity, params Entity[] entities) =>
            CreateSut(identity, new EntityBuilder(entities));

        private static StatBuilder CreateSut(string identity, IEntityBuilder entityBuilder) =>
            new StatBuilder(identity, entityBuilder);
    }
}