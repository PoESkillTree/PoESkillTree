using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;
using static PoESkillTree.Computation.Builders.Tests.Stats.StatBuilderHelper;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    // Also tests Stat and the ICoreStatBuilder implementations through StatBuilder
    [TestFixture]
    public class StatBuilderTest
    {
        [Test]
        public void BuildReturnsPassedModifierSource()
        {
            var sut = CreateSut();
            var expected = new ModifierSource.Global();

            var (_, actual, _) = sut.BuildToSingleResult(expected);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildReturnsIdentityValueConverter()
        {
            var sut = CreateSut();
            ValueConverter expected = Funcs.Identity;

            var (_, _, actual) = sut.BuildToSingleResult(new ModifierSource.Global());

            Assert.AreEqual(expected, actual);
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character, Entity.Totem, Entity.Minion)]
        public void BuildReturnsOneStatPerEntity(params Entity[] entities)
        {
            var sut = CreateSut(entities);

            var (actual, _, _) = sut.BuildToSingleResult(new ModifierSource.Global());

            Assert.AreEqual(entities, actual.Select(s => s.Entity));
        }

        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character)]
        public void BuildReturnsStatForPassedEntityIfEntityBuilderBuildsToEmpty(Entity expected)
        {
            var sut = CreateSut();

            var actual = sut.BuildToSingleStat(entity: expected);

            Assert.AreEqual(expected, actual.Entity);
        }

        [Test]
        public void BuildReturnsStatFactoryResult()
        {
            var expected = Mock.Of<IStat>();
            var sut = CreateSut(CreateStatBuilder(expected));

            var actual = sut.BuildToSingleStat();

            Assert.AreSame(expected, actual);
        }

        [TestCase("1234")]
        [TestCase("test")]
        public void StatToStringReturnsPassedIdentityAndEntity(string identity)
        {
            var expected = $"{default(Entity)}.{identity}";
            var sut = CreateSut(identity);

            var actual = sut.BuildToSingleStat().ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestCase("1", Entity.Enemy)]
        [TestCase("2", Entity.Enemy)]
        [TestCase("2", Entity.Character)]
        public void StatEqualsComparesIdentityAndEntity(string identity, Entity entity)
        {
            var comparedStat = Mock.Of<IStat>(s => s.Identity == identity && s.Entity == entity);
            var statBuilder = CreateSut("1", Entity.Enemy);
            var sut = statBuilder.BuildToSingleStat();
            var expected = identity == "1" && entity == Entity.Enemy;

            var actual = sut.Equals(comparedStat);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(1, Entity.Enemy)]
        [TestCase(2, Entity.Character)]
        public void ValueBuildsToCorrectValue(double expected, Entity entity)
        {
            var sut = CreateSut(entity);
            var stat = sut.BuildToSingleStat();
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
            var stat = sut.BuildToSingleStat(entity: entity);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Total, PathDefinition.MainPath) == expected);

            var value = sut.Value.Build(new BuildParameters(null, entity, default));
            var actual = value.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ValueBuildThrowsWithMultipleEntities()
        {
            var sut = CreateSut(Entity.Character, Entity.Enemy);

            Assert.Throws<ParseException>(() => sut.Value.Build());
        }

        [TestCase("stat")]
        [TestCase("test")]
        public void MinimumBuildIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Minimum");
            var sut = CreateSut(identity);

            var actual = sut.Minimum.BuildToSingleStat();

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void MaximumBuildIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Maximum");
            var sut = CreateSut(identity);

            var actual = sut.Maximum.BuildToSingleStat();

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void BuildMinimumIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Minimum");
            var sut = CreateSut(identity).BuildToSingleStat();

            var actual = sut.Minimum;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("stat")]
        public void BuildMaximumIsCorrectStat(string identity)
        {
            var expected = new Stat(identity + ".Maximum");
            var sut = CreateSut(identity).BuildToSingleStat();

            var actual = sut.Maximum;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MaximumMaximumBuildsToNull()
        {
            var sut = CreateSut().Maximum;

            var actual = sut.Maximum.BuildToSingleStat();

            Assert.IsNull(actual);
        }

        [Test]
        public void MinimumMinimumBuildsToNull()
        {
            var sut = CreateSut().Minimum;

            var actual = sut.Minimum.BuildToSingleStat();

            Assert.IsNull(actual);
        }

        [Test]
        public void ForBuildsUsingPassedEntity()
        {
            var expected = Entity.Enemy;
            var sut = CreateSut();

            var actual = sut.For(new EntityBuilder(expected)).BuildToSingleStat().Entity;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CombineWithBuildsToBothStats()
        {
            var left = CreateSut("left");
            var right = CreateSut("right");
            var expected = new[] { new Stat("left"), new Stat("right"), };

            var combined = left.CombineWith(right);
            var (actual, _, _) = combined.BuildToSingleResult();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CombineWithChainsValueConverters()
        {
            var valueBuilders = Helper.MockMany<IValueBuilder>();
            var expected = valueBuilders[2];
            var sut = CreateSut();
            var other1Mock = new Mock<IStatBuilder>();
            other1Mock.Setup(b => b.Build(default))
                .Returns(CreateResult(valueConverter: _ => valueBuilders[1]));
            var other2Mock = new Mock<IStatBuilder>();
            other2Mock.Setup(b => b.Build(default))
                .Returns(CreateResult(valueConverter: _ => valueBuilders[2]));

            var combined = sut.CombineWith(other1Mock.Object).CombineWith(other2Mock.Object);
            var (_, _, actualConverter) = combined.BuildToSingleResult();
            var actual = actualConverter(valueBuilders[0]);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CombineWithChainsModifierSource()
        {
            var sources = new ModifierSource[]
                { new ModifierSource.Global(), new ModifierSource.Local.Given(), new ModifierSource.Local.Tree(), };
            var sut = CreateSut();
            var other1Mock = new Mock<IStatBuilder>();
            other1Mock.Setup(b => b.Build(Parameters(sources[0])))
                .Returns(CreateResult(modifierSource: sources[1]));
            var other2Mock = new Mock<IStatBuilder>();
            other2Mock.Setup(b => b.Build(Parameters(sources[1])))
                .Returns(CreateResult(modifierSource: sources[2]));

            var combined = sut.CombineWith(other1Mock.Object).CombineWith(other2Mock.Object);
            var (_, actual, _) = combined.Build(Parameters(sources[0])).Single();

            Assert.AreEqual(sources[2], actual);
        }

        [Test]
        public void CombineWithResolveResolvesOther()
        {
            var sut = CreateSut();
            var otherMock = new Mock<IStatBuilder>();

            var combined = sut.CombineWith(otherMock.Object);
            combined.Resolve(null);

            otherMock.Verify(b => b.Resolve(null));
        }

        [Test]
        public void CombineWithForAppliesToOther()
        {
            var sut = CreateSut();
            var otherMock = new Mock<IStatBuilder>();
            var entity = Mock.Of<IEntityBuilder>();

            var combined = sut.CombineWith(otherMock.Object);
            combined.For(entity);

            otherMock.Verify(b => b.For(entity));
        }

        [Test]
        public void CombineWithMinimumAppliesToOther()
        {
            var sut = CreateSut();
            var stat = new Stat("");
            var expected = stat.Minimum;
            var otherMock = new Mock<IStatBuilder>();
            otherMock.Setup(b => b.Build(default))
                .Returns(CreateResult(stat));

            var combined = sut.CombineWith(otherMock.Object);
            var min = combined.Minimum;
            var actual = min.BuildToSingleResult().Stats[1];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CombineWithValueBuildThrows()
        {
            var sut = CreateSut();
            var other = Mock.Of<IStatBuilder>();

            var combined = sut.CombineWith(other);

            Assert.Throws<ParseException>(() => combined.Value.Build());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithConditionBuildReturnsCorrectResult(bool cond)
        {
            var expectedStat = new Stat("1");
            var inputValue = new Constant(2);
            var inputValueBuilder = new ValueBuilderImpl(inputValue);
            var conditionValue = new Constant(cond);
            var expectedValue = cond ? (NodeValue?) 2 : null;
            var convertedStatBuilder = new Mock<IStatBuilder>();
            convertedStatBuilder.Setup(b => b.Build(default))
                .Returns(CreateResult(expectedStat));
            var condition = new Mock<IConditionBuilder>();
            condition.Setup(b => b.Build(default))
                .Returns(new ConditionBuilderResult(_ => convertedStatBuilder.Object, conditionValue));
            var sut = CreateSut();

            var sutWithCondition = sut.WithCondition(condition.Object);
            var actual = sutWithCondition.BuildToSingleResult();
            var actualStat = actual.Stats.Single();
            var actualValue = actual.ValueConverter(inputValueBuilder).Build(default).Calculate(null);

            Assert.AreEqual(expectedStat, actualStat);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void ChanceToDoubleBuildsToCorrectStats()
        {
            var expected = new Stat("stat.ChanceToDouble");
            var sut = CreateSut("stat");

            var statBuilder = sut.ChanceToDouble;
            var actual = statBuilder.BuildToSingleStat();

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(typeof(int), actual.DataType);
        }

        [Test]
        public void ConvertToBuildsToCorrectStats()
        {
            var expected = new[]
            {
                new Stat("s1.ConvertTo(t)"), new Stat("s1.Conversion"), new Stat("s1.SkillConversion"),
                new Stat("s2.ConvertTo(t)"), new Stat("s2.Conversion"), new Stat("s2.SkillConversion"),
            };
            var sources = new[] { CreateStatBuilder("s1"), CreateStatBuilder("s2"), };
            var target = CreateStatBuilder("t");
            var sut = CreateSut(sources[0]).CombineWith(CreateSut(sources[1]));

            var statBuilder = sut.ConvertTo(CreateSut(target));
            var actual = statBuilder.BuildToSingleResult().Stats;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Select(_ => typeof(int)), actual.Select(s => s.DataType));
        }

        [Test]
        public void GainsAsBuildsToCorrectStats()
        {
            var expected = new[]
            {
                new Stat("s1.GainAs(t)"),
                new Stat("s2.GainAs(t)"),
            };
            var sources = new[] { CreateStatBuilder("s1"), CreateStatBuilder("s2"), };
            var target = CreateStatBuilder("t");
            var sut = CreateSut(sources[0]).CombineWith(CreateSut(sources[1]));

            var statBuilder = sut.GainAs(CreateSut(target));
            var actual = statBuilder.BuildToSingleResult().Stats;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected.Select(_ => typeof(int)), actual.Select(s => s.DataType));
        }

        [TestCase(null, false)]
        [TestCase(1.0, true)]
        public void IsSetCalculatesCorrectValue(double? input, bool expected)
        {
            var stat = new Stat("s");
            var results = new[] { new StatBuilderResult(new[] { stat }, null, Funcs.Identity), };
            var coreStatBuilder = Mock.Of<ICoreStatBuilder>(b => b.Build(default) == results);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) input);
            var sut = CreateSut(coreStatBuilder);

            var conditionBuilder = sut.IsSet;
            var actual = conditionBuilder.Build().Value.Calculate(context);

            Assert.AreEqual(expected, actual.IsTrue());
        }

        [Test]
        public void ConvertToIsNotSubClass()
        {
            var sut = new SubStatBuilder(new StatFactory(), null);

            var actual = sut.ConvertTo(Mock.Of<IStatBuilder>());

            Assert.IsInstanceOf<StatBuilder>(actual);
            Assert.IsNotInstanceOf<SubStatBuilder>(actual);
        }

        [Test]
        public void GainAsIsNotSubClass()
        {
            var sut = new SubStatBuilder(new StatFactory(), null);

            var actual = sut.GainAs(Mock.Of<IStatBuilder>());

            Assert.IsInstanceOf<StatBuilder>(actual);
            Assert.IsNotInstanceOf<SubStatBuilder>(actual);
        }

        private static StatBuilder CreateSut(params Entity[] entities) => CreateSut("", entities);

        private static StatBuilder CreateSut(string identity, params Entity[] entities)
        {
            var entityBuilder = entities.Any()
                ? (IEntityBuilder) new EntityBuilder(entities)
                : new ModifierSourceEntityBuilder();
            return CreateSut(identity, entityBuilder);
        }

        private static StatBuilder CreateSut(string identity, IEntityBuilder entityBuilder) =>
            CreateSut(CreateStatBuilder(identity, entityBuilder));

        private static StatBuilder CreateSut(ICoreStatBuilder coreStatBuilder) =>
            new StatBuilder(new StatFactory(), coreStatBuilder);

        private static IReadOnlyList<StatBuilderResult> CreateResult(
            IStat stat = null, ModifierSource modifierSource = null,
            ValueConverter valueConverter = null)
        {
            var stats = stat == null ? new IStat[0] : new[] { stat };
            return new[]
            {
                new StatBuilderResult(stats, modifierSource, valueConverter ?? Funcs.Identity)
            };
        }

        private static BuildParameters Parameters(ModifierSource modifierSource) =>
            default(BuildParameters).With(modifierSource);

        private class SubStatBuilder : StatBuilder
        {
            public SubStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
                : base(statFactory, coreStatBuilder)
            {
            }

            protected override IStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
                new SubStatBuilder(StatFactory, coreStatBuilder);
        }
    }
}