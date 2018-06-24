using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class DamageRelatedStatBuilderTest
    {
        [Test]
        public void BuildReturnsCorrectResult()
        {
            var expectedResultCount = DamageSources.Count + Ailments.Count;
            var expectedStatCount = expectedResultCount + AttackDamageHands.Count - 1;
            var sut = CreateSut();

            var results = sut.Build(default, ModifierSource).Select(r => r.Stats).ToList();
            var stats = results.Flatten().ToList();

            Assert.That(results, Has.Exactly(expectedResultCount).Items);
            Assert.That(stats, Has.Exactly(expectedStatCount).Items);
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

        private static IDamageRelatedStatBuilder CreateSut(string identity = "test") =>
            StatBuilderUtils.DamageRelatedFromIdentity(new StatFactory(), identity, typeof(double));

        private static readonly ModifierSource ModifierSource = new ModifierSource.Global();

        private static readonly IReadOnlyList<DamageSource> DamageSources =
            Enums.GetValues<DamageSource>().ToList();

        private static readonly IReadOnlyList<Ailment> Ailments =
            Enums.GetValues<Ailment>().ToList();

        private static readonly IReadOnlyList<AttackDamageHand> AttackDamageHands =
            Enums.GetValues<AttackDamageHand>().ToList();
    }
}