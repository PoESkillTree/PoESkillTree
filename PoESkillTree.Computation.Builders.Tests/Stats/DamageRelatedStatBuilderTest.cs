using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class DamageRelatedStatBuilderTest
    {
        [Test]
        public void BuildReturnsCorrectResult()
        {
            var expectedCount = FullResultCount;
            var sut = CreateSut();

            var stats = BuildToStats(sut, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
            foreach (var (i, attackDamageHand) in AttackDamageHands.Index())
            {
                var expected = $"test.{DamageSource.Attack}.{attackDamageHand}.Skill";
                Assert.AreEqual(expected, stats[i].Identity);
            }
            var offset = AttackDamageHands.Count;
            foreach (var (i, damageSource) in DamageSources.Except(DamageSource.Attack).Index())
            {
                var expected = $"test.{damageSource}.Skill";
                Assert.AreEqual(expected, stats[i + offset].Identity);
            }
            offset += DamageSources.Count - 1;
            foreach (var attackDamageHand in AttackDamageHands)
            {
                foreach (var (i, ailment) in Ailments.Index())
                {
                    var expected = $"test.{DamageSource.Attack}.{attackDamageHand}.{ailment}";
                    Assert.AreEqual(expected, stats[offset + i].Identity);
                }
                offset += Ailments.Count;
            }
            foreach (var damageSource in DamageSources.Except(DamageSource.Attack, DamageSource.OverTime))
            {
                foreach (var (i, ailment) in Ailments.Index())
                {
                    var expected = $"test.{damageSource}.{ailment}";
                    Assert.AreEqual(expected, stats[offset + i].Identity);
                }
                offset += Ailments.Count;
            }
        }

        [Test]
        public void WithSpellBuildsToCorrectResult()
        {
            var expectedCount = FullResultCount;
            var sut = CreateSut();

            var stats = BuildToStats(sut.With(DamageSource.Spell), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
            Assert.AreEqual("test.Spell.Skill", stats[0].Identity);
            Assert.AreEqual("test.Attack.MainHand.Skill", stats[1].Identity);
            Assert.AreEqual("test.Attack.MainHand.Ignite", stats[SkillResultCount].Identity);
        }

        [Test]
        public void WithOverTimeBuildsToCorrectResult()
        {
            var expectedCount = FullResultCount;
            var sut = CreateSut();

            var stats = BuildToStats(sut.With(DamageSource.OverTime), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
            Assert.AreEqual("test.OverTime.Skill", stats[0].Identity);
            Assert.AreEqual("test.Attack.MainHand.Ignite", stats[1].Identity);
            Assert.AreEqual("test.Attack.MainHand.Skill", stats[1 + AilmentsResultCount].Identity);
        }

        [Test]
        public void WithDamageSourceThrowsIfAlreadyRestricted()
        {
            var sut = CreateSut().With(DamageSource.Attack);

            Assert.Throws<ParseException>(() => sut.With(DamageSource.Spell));
        }

        [Test]
        public void WithHitsBuildsToCorrectResult()
        {
            var expectedCount = SkillResultCount - 1;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithHits, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithHitsAndAilmentsBuildsToCorrectResult()
        {
            var expectedCount = FullResultCount - 1;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithHitsAndAilments, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAilmentBuildsToCorrectResult()
        {
            var expectedCount = SkillResultCount - 1;
            var sut = CreateSut();
            var ailmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Build() == Ailment.Bleed);

            var stats = BuildToStats(sut.With(ailmentBuilder), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAilmentThrowsIfAlreadyRestricted()
        {
            var ailmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Build() == Ailment.Bleed);
            var sut = CreateSut().With(ailmentBuilder);

            Assert.Throws<ParseException>(() => sut.With(ailmentBuilder));
        }

        [Test]
        public void WithAilmentsBuildsToCorrectResult()
        {
            var expectedCount = AilmentsResultCount;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithAilments, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithSkillsBuildsToCorrectResult()
        {
            var expectedCount = FullResultCount;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithSkills, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAttackDamageHandBuildsToCorrectResult()
        {
            var expectedCount = 1 + Ailments.Count;
            var sut = CreateSut();

            var stats = BuildToStats(sut.With(AttackDamageHand.OffHand), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAttackDamageHandThrowsIfAlreadyRestricted()
        {
            var sut = CreateSut().With(AttackDamageHand.MainHand);

            Assert.Throws<ParseException>(() => sut.With(AttackDamageHand.OffHand));
        }

        [Test]
        public void WithAttackDamageHandThrowsIfRestrictedToOtherDamageSource()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            Assert.Throws<ParseException>(() => sut.With(AttackDamageHand.OffHand));
        }

        [Test]
        public void WithAilmentCanBeResolved()
        {
            var ailment = Ailment.Bleed;
            var resolvedAilmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Build() == ailment);
            var ailmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Resolve(null) == resolvedAilmentBuilder);
            var sut = CreateSut();

            var resolved = sut.With(ailmentBuilder).Resolve(null);
            var stat = BuildToStats(resolved, SkillResultCount - 1)[0];

            StringAssert.Contains(ailment.ToString(), stat.Identity);
        }

        [Test]
        public void ApplyModifiersToBuildsToCorrectResults()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            var stats = BuildToStats(sut.ApplyModifiersToSkills(DamageSource.Attack, Form.Increase, Form.More), 1);

            Assert.That(stats, Has.Exactly(2).Items);
            Assert.AreEqual("test.Spell.Skill.ApplyModifiersToSkills(Attack for form Increase)", stats[0].Identity);
        }

        [Test]
        public void ApplyModifiersToAilmentsBuildsToCorrectResults()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            var stats = BuildToStats(sut.ApplyModifiersToAilments(Form.Increase, Form.More), 1);

            Assert.That(stats, Has.Exactly(2).Items);
        }

        [Test]
        public void MinimumBuildsToCorrectResults()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            var stats = BuildToStats(sut.Minimum, 1);

            Assert.That(stats, Has.Exactly(1).Items);
            Assert.AreEqual("test.Spell.Skill.Minimum", stats[0].Identity);
        }

        [Test]
        public void WithSpellsBuildsToCorrectValueConverters()
        {
            var valueBuilder = new ValueBuilderImpl(10);
            var statFactory = new StatFactory();
            var spellDamageStat =
                statFactory.ConcretizeDamage(new Stat("test"), new SkillDamageSpecification(DamageSource.Spell));
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(statFactory.ApplyModifiersToSkillDamage(spellDamageStat, DamageSource.Attack, default),
                    NodeType.Total, PathDefinition.MainPath) == new NodeValue(50) &&
                c.GetValue(statFactory.ApplyModifiersToSkillDamage(spellDamageStat, DamageSource.Secondary, default),
                    NodeType.Total, PathDefinition.MainPath) == new NodeValue(20) &&
                c.GetValue(statFactory.ApplyModifiersToSkillDamage(spellDamageStat, DamageSource.OverTime, default),
                    NodeType.Total, PathDefinition.MainPath) == null &&
                c.GetValue(statFactory.ApplyModifiersToAilmentDamage(spellDamageStat, default),
                    NodeType.Total, PathDefinition.MainPath) == new NodeValue(10));
            var sut = CreateSut();

            var values = sut.With(DamageSource.Spell)
                .Build(default, ModifierSource)
                .Select(r => r.ValueConverter(valueBuilder))
                .Select(b => b.Build().Calculate(context))
                .ToList();

            Assert.AreEqual(new NodeValue(10), values[0]); // Spell
            Assert.AreEqual(new NodeValue(5), values[1]); // Attack.MainHand
            Assert.AreEqual(new NodeValue(2), values[3]); // Secondary
            Assert.AreEqual(null, values[4]); // OverTime
            Assert.AreEqual(new NodeValue(1), values[SkillResultCount]); // Ailment.Ignite
            // ... other ApplyModifiersToAilmentDamage
        }

        [Test]
        public void WithSpellsValueConverterThrowsIfCompositeCoreStatBuilderHasDifferentApplyModifiersToValues()
        {
            var valueBuilder = new ValueBuilderImpl(10);
            var statFactory = new StatFactory();
            var stat1 = statFactory.ConcretizeDamage(new Stat("1"), new SkillDamageSpecification(DamageSource.Spell));
            var stat2 = statFactory.ConcretizeDamage(new Stat("2"), new SkillDamageSpecification(DamageSource.Spell));
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(statFactory.ApplyModifiersToSkillDamage(stat1, DamageSource.Attack, default),
                    NodeType.Total, PathDefinition.MainPath) == new NodeValue(10) &&
                c.GetValue(statFactory.ApplyModifiersToSkillDamage(stat2, DamageSource.Attack, default),
                    NodeType.Total, PathDefinition.MainPath) == new NodeValue(20));
            var sut = DamageRelatedStatBuilder.Create(statFactory, new CompositeCoreStatBuilder(
                    LeafCoreStatBuilder.FromIdentity(statFactory, "1", typeof(double)),
                    LeafCoreStatBuilder.FromIdentity(statFactory, "2", typeof(double))),
                canApplyToSkillDamage: true);

            var values = sut.With(DamageSource.Spell)
                .Build(default, ModifierSource)
                .Select(r => r.ValueConverter(valueBuilder).Build()).ToList();

            Assert.Throws<ParseException>(() => values[1].Calculate(context));
        }

        [Test]
        public void WithHitsAndSpellsBuildsToCorrectResult()
        {
            var expectedCount = 1;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithHits.With(DamageSource.Spell), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void ApplyModifiersToSkillsThrowsIfCantApplyToSkillDamage()
        {
            var sut = CreateSut(canApplyToSkillDamage: false);

            Assert.Throws<ParseException>(() => sut.ApplyModifiersToSkills(DamageSource.Attack, Form.More));
        }

        [Test]
        public void ApplyModifiersToAilmentsThrowsIfCantApplyToSkillDamage()
        {
            var sut = CreateSut(canApplyToAilmentDamage: false);

            Assert.Throws<ParseException>(() => sut.ApplyModifiersToAilments(Form.More));
        }

        [Test]
        public void ValueBuildThrowsParseException()
        {
            var sut = CreateSut(canApplyToSkillDamage: false, canApplyToAilmentDamage: false);

            var value = sut.Value;

            Assert.Throws<ParseException>(() => value.Build());
        }

        private static IDamageRelatedStatBuilder CreateSut(string identity = "test",
            bool canApplyToSkillDamage = true, bool canApplyToAilmentDamage = true) =>
            StatBuilderUtils.DamageRelatedFromIdentity(new StatFactory(), identity, typeof(double),
                canApplyToSkillDamage, canApplyToAilmentDamage);

        private static readonly ModifierSource ModifierSource = new ModifierSource.Global();

        private static readonly IReadOnlyList<DamageSource> DamageSources =
            Enums.GetValues<DamageSource>().ToList();

        private static readonly IReadOnlyList<Ailment> Ailments = AilmentConstants.DamagingAilments;

        private static readonly IReadOnlyList<AttackDamageHand> AttackDamageHands =
            Enums.GetValues<AttackDamageHand>().ToList();

        private static readonly int SkillResultCount = AttackDamageHands.Count + DamageSources.Count - 1;

        private static readonly int AilmentsResultCount = (SkillResultCount - 1) * Ailments.Count;

        private static readonly int FullResultCount = SkillResultCount + AilmentsResultCount;

        private static IReadOnlyList<IStat>
            BuildToStats(IStatBuilder statBuilder, int expectedResultCount)
        {
            var results = statBuilder.Build(default, ModifierSource).Select(r => r.Stats).ToList();
            Assert.That(results, Has.Exactly(expectedResultCount).Items);
            return results.Flatten().ToList();
        }
    }
}