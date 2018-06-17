using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Tests.Behaviors
{
    [TestFixture]
    public class RegenUncappedSubtotalValueTest
    {
        [Test]
        public void CalculateReturnsCorrectWithDefaultTargets()
        {
            var expected = RegenValueFor(Pool.Life);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(Regen(Pool.Life)) == new[] { Path } &&
                c.GetValue(Regen(Pool.Life), NodeType.PathTotal, Path) == expected &&
                c.GetValue(TargetPool(Pool.Life), NodeType.Total, Path) == TargetValueFor(Pool.Life) &&
                c.GetValue(TargetPool(Pool.Mana), NodeType.Total, Path) == TargetValueFor(Pool.Mana) &&
                c.GetValue(TargetPool(Pool.EnergyShield), NodeType.Total, Path) == TargetValueFor(Pool.EnergyShield));
            var sut = CreateSut();

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateReturnsCorrectWithAllLifeTargets()
        {
            var expected = RegenValueFor(Pool.Life) + RegenValueFor(Pool.Mana) + RegenValueFor(Pool.EnergyShield);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(Regen(Pool.Life)) == new[] { Path } &&
                c.GetValue(Regen(Pool.Life), NodeType.PathTotal, Path) == RegenValueFor(Pool.Life) &&
                c.GetPaths(Regen(Pool.Mana)) == new[] { Path } &&
                c.GetValue(Regen(Pool.Mana), NodeType.PathTotal, Path) == RegenValueFor(Pool.Mana) &&
                c.GetPaths(Regen(Pool.EnergyShield)) == new[] { Path } &&
                c.GetValue(Regen(Pool.EnergyShield), NodeType.PathTotal, Path) == RegenValueFor(Pool.EnergyShield) &&
                c.GetValue(TargetPool(Pool.Life), NodeType.Total, Path) == TargetValueFor(Pool.Life) &&
                c.GetValue(TargetPool(Pool.Mana), NodeType.Total, Path) == TargetValueFor(Pool.Life) &&
                c.GetValue(TargetPool(Pool.EnergyShield), NodeType.Total, Path) == TargetValueFor(Pool.Life));
            var sut = CreateSut();

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateConsidersAllPaths()
        {
            var expected = 2 * RegenValueFor(Pool.Mana);
            var paths = new[] { Path, new PathDefinition(new ModifierSource.Local.Given()), };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(Regen(Pool.Mana)) == paths &&
                c.GetValue(Regen(Pool.Mana), NodeType.PathTotal, paths[0]) == RegenValueFor(Pool.Mana) &&
                c.GetValue(Regen(Pool.Mana), NodeType.PathTotal, paths[1]) == RegenValueFor(Pool.Mana) &&
                c.GetValue(TargetPool(Pool.Life), NodeType.Total, Path) == TargetValueFor(Pool.Mana) &&
                c.GetValue(TargetPool(Pool.Mana), NodeType.Total, Path) == TargetValueFor(Pool.Life) &&
                c.GetValue(TargetPool(Pool.EnergyShield), NodeType.Total, Path) == TargetValueFor(Pool.EnergyShield));
            var sut = CreateSut();

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateOnlyManipulatesContextForRegenStat()
        {
            var expected = new NodeValue(42);
            var unrelatedStat = new Stat("a");
            var paths = new[] { new PathDefinition(new ModifierSource.Local.Skill()), };
            var transformedValue = new FunctionalValue(c => c.GetValues(unrelatedStat, NodeType.PathTotal).Sum(), "");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(unrelatedStat) == paths &&
                c.GetValue(unrelatedStat, NodeType.PathTotal, paths[0]) == expected);
            var sut = CreateSut(transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateOnlyManipulatesContextForPathTotal()
        {
            var expected = new NodeValue(42);
            var transformedValue =
                new FunctionalValue(c => c.GetValues(Regen(Pool.Life), NodeType.Base).Sum(), "");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetPaths(Regen(Pool.Life)) == new[] { Path } &&
                c.GetValue(Regen(Pool.Life), NodeType.Base, Path) == expected &&
                c.GetValue(TargetPool(Pool.Life), NodeType.Total, Path) == TargetValueFor(Pool.Life) &&
                c.GetValue(TargetPool(Pool.Mana), NodeType.Total, Path) == TargetValueFor(Pool.Mana) &&
                c.GetValue(TargetPool(Pool.EnergyShield), NodeType.Total, Path) == TargetValueFor(Pool.EnergyShield));
            var sut = CreateSut(transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static IStat Regen(Pool pool) => new Stat($"{pool}.Regen");
        private static IStat TargetPool(Pool pool) => new Stat($"{pool}.TargetPool");
        private static NodeValue RegenValueFor(Pool pool) => new NodeValue((double) pool + 1);
        private static NodeValue TargetValueFor(Pool pool) => new NodeValue((double) pool);
        private static PathDefinition Path => PathDefinition.MainPath;

        private static RegenUncappedSubtotalValue CreateSut(IValue transformedValue = null)
        {
            transformedValue = transformedValue ??
                new FunctionalValue(c => c.GetValues(Regen(Pool.Life), NodeType.PathTotal).Sum(), "");
            return new RegenUncappedSubtotalValue(Pool.Life, Regen, TargetPool, transformedValue);
        }
    }
}