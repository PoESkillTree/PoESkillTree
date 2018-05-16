using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class CalculatorTest
    {
        [Test]
        public void SimpleCalculation()
        {
            var sut = Calculator.CreateCalculator();

            var expected = new NodeValue(5);
            var stat = new Stat();

            sut.NewBatchUpdate()
                .AddModifier(stat, Form.BaseAdd, new Constant(expected), new GlobalModifierSource())
                .DoUpdate();

            var actual = sut.NodeRepository.GetNode(stat).Value;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MultipleUpdates()
        {
            var sut = Calculator.CreateCalculator();
            var stat = new Stat();
            var source = new GlobalModifierSource();
            var removedModifier = new Modifier(new[] { stat }, Form.BaseAdd, new Constant(100), source);

            sut.NewBatchUpdate()
                .AddModifier(stat, Form.BaseAdd, new Constant(10), source)
                .DoUpdate();
            sut.NewBatchUpdate()
                .AddModifier(stat, Form.BaseAdd, new Constant(1), source)
                .AddModifier(removedModifier)
                .DoUpdate();
            sut.NewBatchUpdate()
                .RemoveModifier(removedModifier)
                .AddModifier(stat, Form.BaseAdd, new Constant(1000), source)
                .DoUpdate();

            var actual = sut.NodeRepository.GetNode(stat).Value;

            Assert.AreEqual(new NodeValue(1011), actual);
        }

        [Test]
        public void EvasionCalculation()
        {
            var sut = Calculator.CreateCalculator();

            var evasionStat = new Stat();
            var lvlStat = new Stat();
            var dexterityStat = new Stat();
            var globalSource = new GlobalModifierSource();
            var bodySource = new LocalModifierSource();
            var shieldSource = new LocalModifierSource();

            sut.NewBatchUpdate()
                .AddModifier(evasionStat, Form.BaseSet, new Constant(53), globalSource)
                .AddModifier(evasionStat, Form.BaseAdd, new PerStatValue(lvlStat, 3), globalSource)
                .AddModifier(evasionStat, Form.BaseSet, new Constant(1000), bodySource)
                .AddModifier(evasionStat, Form.BaseSet, new Constant(500), shieldSource)
                .AddModifier(evasionStat, Form.Increase, new Constant(100), globalSource)
                .AddModifier(evasionStat, Form.Increase, new PerStatValue(dexterityStat, 1, 5), globalSource)
                .AddModifier(evasionStat, Form.Increase, new Constant(20), shieldSource)
                .AddModifier(evasionStat, Form.More, new Constant(100), bodySource)
                .AddModifier(lvlStat, Form.BaseSet, new Constant(90), globalSource)
                .AddModifier(dexterityStat, Form.BaseSet, new Constant(32), globalSource)
                .AddModifier(dexterityStat, Form.BaseAdd, new Constant(50), globalSource)
                .DoUpdate();
            var lvlTotal = 90 * 3;
            var dexterityTotal = 50 + 32;
            var globalBase = 53 + lvlTotal;
            var globalIncrease = 1 + Math.Ceiling(dexterityTotal / 5.0) / 100;
            var globalTotal = globalBase * (1 + globalIncrease);
            var bodyBase = 1000;
            var bodyTotal = bodyBase * (1 + globalIncrease) * 2;
            var shieldBase = 500;
            var shieldTotal = shieldBase * (1 + globalIncrease + 0.2);
            var evasionTotal = globalTotal + bodyTotal + shieldTotal;

            var actual = sut.NodeRepository.GetNode(evasionStat).Value;

            Assert.AreEqual(new NodeValue(evasionTotal), actual);
        }

        // Left to test:
        // TODO Minimum and Maximum stats
        // TODO Pruning
        // TODO Events
        // TODO Explicitly registered stats
        // TODO Behaviors
        // TODO Conversion paths (requires a bunch of behaviors)


        private class Stat : IStat
        {
            public bool Equals(IStat other) => Equals((object) other);

            public IStat Minimum => null;
            public IStat Maximum => null;
            public bool IsRegisteredExplicitly => false;
            public Type DataType => typeof(double);
            public IEnumerable<IBehavior> Behaviors => Enumerable.Empty<IBehavior>();
        }


        private class Constant : IValue
        {
            private readonly NodeValue? _value;

            public Constant(double value) =>
                _value = new NodeValue(value);

            public Constant(NodeValue? value) =>
                _value = value;

            public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
                _value;
        }

        private class PerStatValue : IValue
        {
            private readonly IStat _stat;
            private readonly double _multiplier;
            private readonly double _divisor;

            public PerStatValue(IStat stat, double multiplier, double divisor = 1)
            {
                _stat = stat;
                _multiplier = multiplier;
                _divisor = divisor;
            }

            public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
                _multiplier * (valueCalculationContext.GetValue(_stat) / _divisor).Select(Math.Ceiling);
        }


        private class LocalModifierSource : IModifierSource
        {
            public LocalModifierSource()
            {
                InfluencingSources = new IModifierSource[] { this, new GlobalModifierSource(), };
            }

            public bool Equals(IModifierSource other) => Equals((object) other);

            public ModifierSourceFirstLevel FirstLevel => ModifierSourceFirstLevel.Local;
            public IReadOnlyList<IModifierSource> InfluencingSources { get; }
            public IModifierSource CanonicalSource => this;
        }
    }


    internal static class BatchUpdateExtensions
    {
        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, IValue value, IModifierSource source) =>
            batch.AddModifier(new[] { stat }, form, value, source);

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, IValue value, IModifierSource source) =>
            batch.AddModifier(new Modifier(stats, form, value, source));
    }
}