using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class DamageStatBuilderTest
    {
        [Test]
        public void TakenBuildsToCorrectResults()
        {
            var expected = "test.Spell.Skill.Taken";
            var sut = CreateSut();

            var taken = sut.Taken.With(DamageSource.Spell);
            var results = taken.Build(default, null).ToList();
            
            Assert.That(results, Has.One.Items);
            var (stats, _, _) = results.Single();
            Assert.That(stats, Has.One.Items);
            var actual = stats.Single().Identity;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WithKeywordHasCorrectValueConverters()
        {
            var keyword = Keyword.Bow;
            var keywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Build() == keyword);
            var valueBuilder = new ValueBuilderImpl(2);
            var context = SetupKeywordContext(keyword);
            var sut = CreateSut();

            var results = sut.WithHits.With(keywordBuilder).Build(default, null).ToList();

            Assert.That(results, Has.Exactly(4).Items);
            var attackValue = results[0].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(new NodeValue(2), attackValue);
            var spellValue = results[2].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(null, spellValue);
        }

        [Test]
        public void NotWithKeywordHasCorrectValueConverters()
        {
            var keyword = Keyword.Bow;
            var keywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Build() == keyword);
            var valueBuilder = new ValueBuilderImpl(2);
            var context = SetupKeywordContext(keyword);
            var sut = CreateSut();

            var results = sut.WithHits.NotWith(keywordBuilder).Build(default, null).ToList();

            Assert.That(results, Has.Exactly(4).Items);
            var attackValue = results[0].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(null, attackValue);
            var spellValue = results[2].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(new NodeValue(2), spellValue);
        }

        [Test]
        public void WithKeywordIsResolved()
        {
            var keyword = Keyword.Bow;
            var keywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Build() == keyword);
            var unresolvedKeywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Resolve(null) == keywordBuilder);
            var valueBuilder = new ValueBuilderImpl(2);
            var context = SetupKeywordContext(keyword);
            var sut = CreateSut();

            var resolved = sut.WithHits.With(unresolvedKeywordBuilder).Resolve(null);
            var results = resolved.Build(default, null).ToList();

            Assert.That(results, Has.Exactly(4).Items);
            var attackValue = results[0].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(new NodeValue(2), attackValue);
            var spellValue = results[2].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(null, spellValue);
        }

        private static IValueCalculationContext SetupKeywordContext(Keyword keyword)
        {
            var statFactory = new StatFactory();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(statFactory.ActiveSkillPartDamageHasKeyword(default, keyword, DamageSource.Attack),
                    NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(statFactory.ActiveSkillPartDamageHasKeyword(default, keyword, DamageSource.Spell),
                    NodeType.Total, PathDefinition.MainPath) == (NodeValue?) false);
            return context;
        }

        private static DamageStatBuilder CreateSut()
        {
            var statFactory = new StatFactory();
            var coreStatBuilder = LeafCoreStatBuilder.FromIdentity(statFactory, "test", typeof(double));
            return new DamageStatBuilder(statFactory, coreStatBuilder);
        }
    }
}