using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Behaviors
{
    [TestFixture]
    public class DamageEffectivenessBaseValueTest
    {
        [TestCase(null, null)]
        [TestCase(1.5, 2)]
        public void CalculateReturnsCorrectResult(double? baseSetEffectiveness, double? baseAddEffectiveness)
        {
            var baseSet = (NodeValue?) 2;
            var baseAdd = (NodeValue?) 3;
            var expected = baseSet * (baseSetEffectiveness ?? 1) + baseAdd * (baseAddEffectiveness ?? 1);

            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(TransformedStat, NodeType.BaseSet, PathDefinition.MainPath) == baseSet &&
                c.GetValue(TransformedStat, NodeType.BaseAdd, PathDefinition.MainPath) == baseAdd &&
                c.GetValue(SetEffectivenessStat, NodeType.Total, PathDefinition.MainPath) ==
                (NodeValue?) baseSetEffectiveness &&
                c.GetValue(AddEffectivenessStat, NodeType.Total, PathDefinition.MainPath) ==
                (NodeValue?) baseAddEffectiveness);
            var sut = CreateSut(
                c => c.GetValue(TransformedStat, NodeType.BaseSet) + c.GetValue(TransformedStat, NodeType.BaseAdd));

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateDoesNotTransformOtherForms()
        {
            var expected = (NodeValue?) 42;
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(TransformedStat, NodeType.Increase, PathDefinition.MainPath) == expected &&
                c.GetValue(SetEffectivenessStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 2 &&
                c.GetValue(AddEffectivenessStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 2);
            var sut = CreateSut(c => c.GetValue(TransformedStat, NodeType.Increase));

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CalculateDoesNotTransformOtherStats()
        {
            var expected = (NodeValue?) 42;
            var otherStat = new Stat("other");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(otherStat, NodeType.BaseSet, PathDefinition.MainPath) == expected &&
                c.GetValue(SetEffectivenessStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 2 &&
                c.GetValue(AddEffectivenessStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 2);
            var sut = CreateSut(c => c.GetValue(otherStat, NodeType.BaseSet));

            var actual = sut.Calculate(context);

            Assert.AreEqual(expected, actual);
        }

        private static DamageEffectivenessBaseValue CreateSut(Func<IValueCalculationContext, NodeValue?> baseCalculate)
            => new DamageEffectivenessBaseValue(TransformedStat, SetEffectivenessStat, AddEffectivenessStat,
                new FunctionalValue(baseCalculate, ""));

        private static readonly IStat TransformedStat = new Stat("transformed");
        private static readonly IStat SetEffectivenessStat = new Stat("setEffectiveness");
        private static readonly IStat AddEffectivenessStat = new Stat("addEffectiveness");
    }
}