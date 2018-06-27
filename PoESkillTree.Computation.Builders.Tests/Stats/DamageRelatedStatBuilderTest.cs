using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
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
            var expectedCount = AttackDamageHands.Count + DamageSources.Count - 1 + Ailments.Count;
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
            foreach (var (i, ailment) in Ailments.Index())
            {
                var expected = $"test.{DamageSource.OverTime}.{ailment}";
                Assert.AreEqual(expected, stats[offset + i].Identity);
            }
        }

        [Test]
        public void WithSpellBuildsToCorrectResult()
        {
            var expectedCount = 1;
            var sut = CreateSut();

            var stats = BuildToStats(sut.With(DamageSource.Spell), expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
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
            var expectedCount = AttackDamageHands.Count + DamageSources.Count - 2;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithHits, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithHitsAndAilmentsBuildsToCorrectResult()
        {
            var expectedCount = AttackDamageHands.Count + DamageSources.Count - 2 + Ailments.Count;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithHitsAndAilments, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAilmentBuildsToCorrectResult()
        {
            var expectedCount = 1;
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
            var expectedCount = Ailments.Count;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithAilments, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithSkillsBuildsToCorrectResult()
        {
            var expectedCount = AttackDamageHands.Count + DamageSources.Count - 1;
            var sut = CreateSut();

            var stats = BuildToStats(sut.WithSkills, expectedCount);

            Assert.That(stats, Has.Exactly(expectedCount).Items);
        }

        [Test]
        public void WithAttackDamageHandBuildsToCorrectResult()
        {
            var expectedCount = 1;
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
        public void WithAilmentCanBeResolved()
        {
            var ailment = Ailment.Bleed;
            var resolvedAilmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Build() == ailment);
            var ailmentBuilder = Mock.Of<IAilmentBuilder>(b => b.Resolve(null) == resolvedAilmentBuilder);
            var sut = CreateSut();

            var resolved = sut.With(ailmentBuilder).Resolve(null);
            var stat = BuildToStats(resolved, 1)[0];

            StringAssert.Contains(ailment.ToString(), stat.Identity);
        }

        [Test]
        public void ApplyModifiersToBuildsToCorrectResults()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            var stats = BuildToStats(sut.ApplyModifiersTo(DamageSource.Attack, Form.Increase, Form.More), 1);

            Assert.That(stats, Has.Exactly(2).Items);
        }

        [Test]
        public void ApplyModifiersToAilmentsBuildsToCorrectResults()
        {
            var sut = CreateSut().With(DamageSource.Spell);

            var stats = BuildToStats(sut.ApplyModifiersToAilments(Form.Increase, Form.More), 1);

            Assert.That(stats, Has.Exactly(2).Items);
        }

        private static IDamageRelatedStatBuilder CreateSut(string identity = "test") =>
            StatBuilderUtils.DamageRelatedFromIdentity(new StatFactory(), identity, typeof(double));

        private static readonly ModifierSource ModifierSource = new ModifierSource.Global();

        private static readonly IReadOnlyList<DamageSource> DamageSources =
            Enums.GetValues<DamageSource>().ToList();

        private static readonly IReadOnlyList<Ailment> Ailments =
            Enums.GetValues<Ailment>().ToList();

        private static readonly IReadOnlyList<AttackDamageHand> AttackDamageHands =
            Enums.GetValues<AttackDamageHand>().ToList();

        private static IReadOnlyList<IStat>
            BuildToStats(IStatBuilder statBuilder, int expectedResultCount)
        {
            var results = statBuilder.Build(default, ModifierSource).Select(r => r.Stats).ToList();
            Assert.That(results, Has.Exactly(expectedResultCount).Items);
            return results.Flatten().ToList();
        }
    }
}