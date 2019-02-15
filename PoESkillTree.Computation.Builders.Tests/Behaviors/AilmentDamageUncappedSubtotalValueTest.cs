using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Tests.Behaviors
{
    [TestFixture]
    public class AilmentDamageUncappedSubtotalValueTest
    {
        [Test]
        public void CalculateReturnsCorrectResult()
        {
            var expected = (NodeValue?) 42;
            var ailmentDamage = new Stat("Damage.Spell.Ignite");
            var skillDamage = ConcretizeDamage(new SkillDamageSpecification(DamageSource.Spell));
            var paths = new[]
            {
                PathDefinition.MainPath,
                new PathDefinition(new ModifierSource.Local.Given()),
                new PathDefinition(new ModifierSource.Local.Given(), new Stat("Damage.Attack")),
            };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(ailmentDamage) == paths.Take(1).ToList() &&
                c.GetPaths(skillDamage) == paths.Skip(2).ToList() &&
                c.GetValue(ailmentDamage, NodeType.PathTotal, paths[0]) == expected.Value - 10 &&
                c.GetValue(ailmentDamage, NodeType.PathTotal, paths[1]) == new NodeValue(10));
            var sut = CreateSut(ailmentDamage, skillDamage);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateReturnsOriginalResultIfValueUsesDifferentStats()
        {
            var expected = (NodeValue?) 43;
            var ailmentDamage = new Stat("Damage.Spell.Ignite");
            var skillDamage = ConcretizeDamage(new SkillDamageSpecification(DamageSource.Spell));
            var unrelated = new Stat("unrelated");
            var paths = new[] { PathDefinition.MainPath, new PathDefinition(new ModifierSource.Local.Given()) };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(unrelated) == paths.Take(1).ToList() &&
                c.GetPaths(skillDamage) == paths.Skip(1).ToList() &&
                c.GetValue(unrelated, NodeType.PathTotal, paths[0]) == expected &&
                c.GetValue(unrelated, NodeType.PathTotal, paths[1]) == new NodeValue(10));
            var transformedValue = new FunctionalValue(c => c.GetValues(unrelated, NodeType.PathTotal).Sum(), "");
            var sut = new AilmentDamageUncappedSubtotalValue(ailmentDamage, skillDamage, transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static AilmentDamageUncappedSubtotalValue CreateSut(IStat ailmentDamage, IStat skillDamage)
        {
            var transformedValue = new FunctionalValue(c => c.GetValues(ailmentDamage, NodeType.PathTotal).Sum(), "");
            return new AilmentDamageUncappedSubtotalValue(ailmentDamage, skillDamage, transformedValue);
        }

        private static IStat ConcretizeDamage(IDamageSpecification spec) =>
            new Stat($"Damage.{spec.StatIdentitySuffix}");
    }
}