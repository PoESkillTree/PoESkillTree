using NUnit.Framework;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Tests.Damage
{
    [TestFixture]
    public class DamageSourceBuildersTest
    {
        [Test]
        public void FromResolvesToSelf()
        {
            var sut = new DamageSourceBuilders();
            var builder = sut.From(DamageSource.Attack);

            var actual = builder.Resolve(BuildersHelper.MockResolveContext());

            Assert.AreEqual(builder, actual);
        }

        [TestCase(DamageSource.Attack)]
        [TestCase(DamageSource.Spell)]
        public void FromBuildsToPassedItemSlot(DamageSource expected)
        {
            var sut = new DamageSourceBuilders();
            var builder = sut.From(expected);

            var actual = builder.Build();

            Assert.AreEqual(expected, actual);
        }
    }
}