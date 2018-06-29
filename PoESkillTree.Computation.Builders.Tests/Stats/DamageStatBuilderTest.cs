using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class DamageStatBuilderTest
    {
        [Test]
        public void TakenBuildsToCorrectResults()
        {
            var expected = "test.Spell.Skill.Taken";
            var statFactory = new StatFactory();
            var coreStatBuilder = LeafCoreStatBuilder.FromIdentity(statFactory, "test", typeof(double));
            var sut = new DamageStatBuilder(statFactory, coreStatBuilder);

            var taken = sut.Taken.With(DamageSource.Spell);
            var results = taken.Build(default, null).ToList();
            
            Assert.That(results, Has.One.Items);
            var (stats, _, _) = results.Single();
            Assert.That(stats, Has.One.Items);
            var actual = stats.Single().Identity;
            Assert.AreEqual(expected, actual);
        }
    }
}