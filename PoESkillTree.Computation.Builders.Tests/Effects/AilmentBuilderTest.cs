using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Effects;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Tests.Effects
{
    [TestFixture]
    public class AilmentBuilderTest
    {
        [Test]
        public void SourceBuildsToCorrectResults()
        {
            var damageTypes =
                Mock.Of<IDamageTypeBuilder>(b => b.BuildDamageTypes() == new[] { DamageType.Fire, DamageType.Cold });
            var sut = new AilmentBuilder(new StatFactory(), Ailment.Bleed);

            var statBuilder = sut.Source(damageTypes);
            var results = statBuilder.Build(default).ToList();

            Assert.That(results, Has.One.Items);
            var stats = results.Single().Stats;
            Assert.That(stats, Has.Exactly(2).Items);
            Assert.AreEqual("Bleed.HasSource.Fire", stats[0].Identity);
            Assert.AreEqual("Bleed.HasSource.Cold", stats[1].Identity);
        }
    }
}