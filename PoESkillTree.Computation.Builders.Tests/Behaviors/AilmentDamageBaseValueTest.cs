using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    [TestFixture]
    public class AilmentDamageBaseValueTest
    {
        [Test]
        public void CalculateOnSpellReturnsCorrectResult()
        {
            var expected = (NodeValue?) (1 + 2);
            var ailmentDamage = new Stat("Damage.Spell.Ignite");
            var skillDamage = ConcretizeDamage(new SkillDamageSpecification(DamageSource.Spell));
            var path = PathDefinition.MainPath;
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.CurrentPath == path &&
                c.GetValue(ailmentDamage, NodeType.BaseSet, path) == new NodeValue(1) &&
                c.GetValue(skillDamage, NodeType.Base, path) == new NodeValue(2));
            var sut = CreateSut(ailmentDamage, skillDamage);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateOnAttackReturnsCorrectResult()
        {
            var expected = (NodeValue?) (1 + 3);
            var ailmentDamage = new Stat("Damage.Attack.MainHand.Ignite");
            var skillDamage = ConcretizeDamage(new SkillAttackDamageSpecification(AttackDamageHand.MainHand));
            var path = PathDefinition.MainPath;
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.CurrentPath == path &&
                c.GetValue(ailmentDamage, NodeType.BaseSet, path) == new NodeValue(1) &&
                c.GetValue(skillDamage, NodeType.Base, path) == new NodeValue(3));
            var sut = CreateSut(ailmentDamage, skillDamage);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateUsesCurrentPathOfContext()
        {
            var skillDamage = new Stat("");
            var path = new PathDefinition(new ModifierSource.Local.Tree());
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.CurrentPath == path &&
                c.GetValue(skillDamage, NodeType.Base, path) == new NodeValue(3));
            var sut = new AilmentDamageBaseValue(skillDamage, new Constant(false));

            var actual = sut.Calculate(context);

            Assert.AreEqual(new NodeValue(3), actual);
        }

        private static AilmentDamageBaseValue CreateSut(IStat ailmentDamage, IStat skillDamage)
        {
            var transformedValue = new FunctionalValue(c => c.GetValue(ailmentDamage, NodeType.BaseSet), "");
            return new AilmentDamageBaseValue(skillDamage, transformedValue);
        }

        private static IStat ConcretizeDamage(IDamageSpecification spec) =>
            new Stat($"Damage.{spec.StatIdentitySuffix}");
    }
}