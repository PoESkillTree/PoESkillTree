using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Conditions
{
    [TestFixture]
    public class ConditionBuildersTest
    {
        [Test]
        public void ForConvertsStatsCorrectly()
        {
            var expected = Mock.Of<IStatBuilder>();
            var entity = Mock.Of<IEntityBuilder>();
            var inStat = Mock.Of<IStatBuilder>(b => b.For(entity) == expected);
            var sut = CreateSut();

            var condition = sut.For(entity);
            var statConverter = condition.Build().StatConverter;
            var actual = statConverter(inStat);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ForBuildsToTrueValue()
        {
            var sut = CreateSut();

            var value = sut.For(Mock.Of<IEntityBuilder>()).Build().Value;

            Assert.IsTrue(value.Calculate(null).IsTrue());
        }

        [Test]
        public void BaseValueComesFromConvertsStatCorrectly()
        {
            var slot = ItemSlot.Amulet;
            var expected = new ModifierSource.Local.Item(slot);
            var expectedParameters = new BuildParameters(expected, default, default);
            var result = new StatBuilderResult(new IStat[0], expected, null);
            var inStat = Mock.Of<IStatBuilder>(b => b.Build(expectedParameters) == new[] { result });
            var sut = CreateSut();

            var condition = sut.BaseValueComesFrom(slot);
            var stat = condition.Build().StatConverter(inStat);
            var (_, actual, _) =
                stat.Build(new BuildParameters(new ModifierSource.Global(), default, default)).Single();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AttackWithHandConvertsStatsCorrectly()
        {
            var expected = Mock.Of<IDamageRelatedStatBuilder>();
            var hand = AttackDamageHand.OffHand;
            var inStat =
                Mock.Of<IDamageRelatedStatBuilder>(b => b.WithSkills.With(DamageSource.Attack).With(hand) == expected);
            var sut = CreateSut();

            var statConverter = sut.AttackWith(hand).Build().StatConverter;
            var actual = statConverter(inStat);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AttackWithHandThrowsWhenConvertingNonDamageRelatedStat()
        {
            var inStat = Mock.Of<IStatBuilder>();
            var sut = CreateSut();

            var statConverter = sut.AttackWith(AttackDamageHand.MainHand).Build().StatConverter;

            Assert.Throws<ParseException>(() => statConverter(inStat));
        }

        [Test]
        public void WithSkillConvertsDamageStatsCorrectly()
        {
            var expected = Mock.Of<IDamageRelatedStatBuilder>();
            var skill = MockSkillBuilder();
            var inStat = Mock.Of<IDamageRelatedStatBuilder>(b => b.WithSkills == expected);
            var sut = CreateSut();

            var statConverter = sut.With(skill).Build().StatConverter;
            var actual = statConverter(inStat);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WithSkillsDoesNotConvertNonDamageStats()
        {
            var expected = Mock.Of<IStatBuilder>();
            var skill = MockSkillBuilder();
            var sut = CreateSut();

            var statConverter = sut.With(skill).Build().StatConverter;
            var actual = statConverter(expected);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(42, 42)]
        [TestCase(43, 42)]
        public void WithSkillIsCorrectValue(int activeSkillId, int skillId)
        {
            var expected = activeSkillId == skillId;
            var skill = MockSkillBuilder(skillId);
            var activeSkillStat = new StatFactory().MainSkillId(default);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(activeSkillStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(activeSkillId));
            var sut = CreateSut();

            var value = sut.With(skill).Build().Value;
            var actual = value.Calculate(context);

            Assert.AreEqual(expected, actual.IsTrue());
        }

        [TestCase(Keyword.Projectile)]
        [TestCase(Keyword.Aura)]
        public void WithKeywordIsCorrectValue(Keyword actualKeyword)
        {
            var expectedKeyword = Keyword.Aura;
            var expected = expectedKeyword == actualKeyword;
            var keyword = Mock.Of<IKeywordBuilder>(b => b.Build(default) == actualKeyword);
            var hasKeywordStat = new StatFactory().MainSkillHasKeyword(default, expectedKeyword);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(hasKeywordStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true);
            var sut = CreateSut();

            var value = sut.With(keyword).Build().Value;
            var actual = value.Calculate(context);

            Assert.AreEqual(expected, actual.IsTrue());
        }

        [Test]
        public void WithKeywordResolvesKeyword()
        {
            var keywordMock = new Mock<IKeywordBuilder>();
            var context = BuildersHelper.MockResolveContext();
            var sut = CreateSut();
            var condition = sut.With(keywordMock.Object);

            condition.Resolve(context);

            keywordMock.Verify(b => b.Resolve(context));
        }

        [Test]
        public void DamageTakenConvertsStatsCorrectly()
        {
            var expected = Mock.Of<IDamageStatBuilder>();
            var inStat = Mock.Of<IDamageStatBuilder>(b => b.Taken == expected);
            var sut = CreateSut();

            var statConverter = sut.DamageTaken.Build().StatConverter;
            var actual = statConverter(inStat);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DamageTakenThrowsWhenConvertingNonDamagedStat()
        {
            var inStat = Mock.Of<IDamageRelatedStatBuilder>();
            var sut = CreateSut();

            var statConverter = sut.DamageTaken.Build().StatConverter;

            Assert.Throws<ParseException>(() => statConverter(inStat));
        }

        private static ISkillBuilder MockSkillBuilder(int skillId = 42) =>
            Mock.Of<ISkillBuilder>(b => b.SkillId == new ValueBuilder(new ValueBuilderImpl(skillId)));

        private static ConditionBuilders CreateSut() => new ConditionBuilders(new StatFactory());
    }
}