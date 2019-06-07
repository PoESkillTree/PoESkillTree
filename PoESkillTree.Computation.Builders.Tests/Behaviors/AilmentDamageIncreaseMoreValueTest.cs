using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    [TestFixture]
    public class AilmentDamageIncreaseMoreValueTest
    {
        [TestCase(Form.Increase)]
        [TestCase(Form.More)]
        public void CalculateReturnsCorrectResultWithMainPath(Form form)
        {
            NodeValue? expected = new NodeValue((int) form);
            var fireDamage = AilmentDamage(DamageType.Fire);
            var coldDamage = AilmentDamage(DamageType.Cold);
            var transformedValue = new FunctionalValue(c => c.GetValues(form, coldDamage).Sum(), "");
            var paths = new[]
            {
                (coldDamage, PathDefinition.MainPath),
                (fireDamage, PathDefinition.MainPath),
            };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValues(form, paths) == new List<NodeValue?> { expected } &&
                c.GetValue(DealtDamageType, NodeType.Total, PathDefinition.MainPath) ==
                new NodeValue((int) DamageType.Fire));
            var sut = CreateSut(coldDamage, transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateReturnsCorrectResultWithMultiplePaths()
        {
            var expected = (NodeValue?) 42;
            var fireDamage = AilmentDamage(DamageType.Fire);
            var coldDamage = AilmentDamage(DamageType.Cold);
            var lightningDamage = AilmentDamage(DamageType.Lightning);
            var originalPaths = new[]
            {
                (coldDamage, new PathDefinition(new ModifierSource.Local.Given())),
                (lightningDamage, PathDefinition.MainPath),
            };
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.Increase, originalPaths).Sum(), "");
            var paths = new[]
            {
                (coldDamage, new PathDefinition(new ModifierSource.Local.Given())),
                (fireDamage, new PathDefinition(new ModifierSource.Local.Given())),
                (lightningDamage, PathDefinition.MainPath),
            };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValues(Form.Increase, paths) == new List<NodeValue?> { expected } &&
                c.GetValue(DealtDamageType, NodeType.Total, PathDefinition.MainPath) ==
                new NodeValue((int) DamageType.Fire));
            var sut = CreateSut(coldDamage, transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateReturnsOriginalResultWithBaseAddForm()
        {
            var expected = (NodeValue?) 42;
            var coldDamage = AilmentDamage(DamageType.Cold);
            var transformedValue = new FunctionalValue(c => c.GetValues(Form.BaseAdd, coldDamage).Sum(), "");
            var paths = new[] { (coldDamage, PathDefinition.MainPath), };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValues(Form.BaseAdd, paths) == new List<NodeValue?> { expected } &&
                c.GetValue(DealtDamageType, NodeType.Total, PathDefinition.MainPath) ==
                new NodeValue((int) DamageType.Fire));
            var sut = CreateSut(coldDamage, transformedValue);

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static AilmentDamageIncreaseMoreValue CreateSut(IStat coldDamage, IValue transformedValue) =>
            new AilmentDamageIncreaseMoreValue(coldDamage, DealtDamageType, AilmentDamage, transformedValue);

        private static IStat AilmentDamage(DamageType damageType) =>
            new Stat($"{damageType}.Damage.Ignite");

        private static readonly IStat DealtDamageType = new Stat("Ingite.DamageType");
    }
}