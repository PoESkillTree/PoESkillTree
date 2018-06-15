using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [TestFixture]
    public class ConversionTest
    {
        // This test tests the behaviors necessary for stat conversions.

        private ICalculator _sut;
        private IStat _bar;
        private IStat _foo;
        private IStat _barFooConversion;
        private IStat _barFooGain;
        private IStat _barConversion;
        private IStat _barSkillConversion;

        [SetUp]
        public void SetUp()
        {
            var fooPathTotalBehavior = new Behavior(
                new LazyStatEnumerable(() => _foo),
                new[] { NodeType.PathTotal },
                BehaviorPathInteraction.ConversionPathsOnly,
                new ValueTransformation(v => new ConversionTargetPathTotalValue(_barFooConversion, _barFooGain, v)));
            var fooUncappedSubtotalBehavior = new Behavior(
                new LazyStatEnumerable(() => _foo),
                new[] { NodeType.UncappedSubtotal },
                BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new ConversionTargeUncappedSubtotalValue(_foo, _bar, v)));
            var barPathTotalBehavior = new Behavior(
                new LazyStatEnumerable(() => _bar),
                new[] { NodeType.PathTotal },
                BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new ConversionSourcePathTotalValue(_barConversion, v)));
            var barFooConversionUncappedSubtotalValue = new Behavior(
                new LazyStatEnumerable(() => _barFooConversion),
                new[] { NodeType.UncappedSubtotal }, BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new ConvertToUncappedSubtotalValue(
                    _barFooConversion, _barConversion, _barSkillConversion, v)));
            var barSkillConversionUncappedSubtotalBehavior = new Behavior(
                new LazyStatEnumerable(() => _barSkillConversion),
                new[] { NodeType.UncappedSubtotal }, BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new SkillConversionUncappedSubtotalValue(_barSkillConversion, v)));

            _sut = Calculator.CreateCalculator();
            _bar = new Stat("Bar");
            _foo = new Stat("Foo");
            _barFooConversion = new Stat("BarFooConversion", behaviors: new[]
            {
                fooPathTotalBehavior, fooUncappedSubtotalBehavior, barPathTotalBehavior,
                barFooConversionUncappedSubtotalValue
            });
            _barFooGain = new Stat("BarFooGain", behaviors: new[]
            {
                fooPathTotalBehavior, fooUncappedSubtotalBehavior
            });
            _barConversion = new Stat("BarConversion");
            _barSkillConversion = new Stat("BarSkillConversion", behaviors: new[]
            {
                barSkillConversionUncappedSubtotalBehavior
            });
        }

        [Test]
        public void SimpleGain()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(_barFooGain, Form.BaseAdd, 50)
                .DoUpdate();

            Assert.AreEqual(new NodeValue(1.5), GetValue(_foo));
        }

        [Test]
        public void SimpleConversion()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(new[] { _barFooConversion, _barConversion, _barSkillConversion }, Form.BaseAdd, 100.0 / 3)
                .DoUpdate();

            Assert.AreEqual(new NodeValue(1), GetValue(_foo));
            Assert.AreEqual(new NodeValue(2), GetValue(_bar));
        }

        [Test]
        public void Complex()
        {
            var barFooConversion = new[] { _barFooConversion, _barConversion, _barSkillConversion };
            var localSource = new ModifierSource.Local.Given();
            var skillSource = new ModifierSource.Local.Skill();
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(_bar, Form.BaseAdd, 2, localSource)
                .AddModifier(barFooConversion, Form.BaseAdd, 50, skillSource)
                .AddModifier(barFooConversion, Form.BaseAdd, 30)
                .AddModifier(barFooConversion, Form.BaseAdd, 30)
                .AddModifier(_barFooGain, Form.BaseAdd, 20)
                .AddModifier(_foo, Form.Increase, 50)
                .AddModifier(_foo, Form.Increase, 50, localSource)
                .AddModifier(_foo, Form.BaseAdd, 1)
                .AddModifier(_foo, Form.BaseAdd, 2, localSource)
                .DoUpdate();

            var globalPath = (3 * (1 + 0.2) + 1) * 1.5;
            var localPath = (2 * (1 + 0.2) + 2) * 2;
            Assert.AreEqual(new NodeValue(globalPath + localPath), GetValue(_foo));
            Assert.AreEqual(new NodeValue(0), GetValue(_bar));
        }

        private NodeValue? GetValue(IStat stat) => _sut.NodeRepository.GetNode(stat).Value;

        private class LazyStatEnumerable : IEnumerable<IStat>
        {
            private readonly Lazy<IEnumerable<IStat>> _lazyStats;

            public LazyStatEnumerable(Func<IStat> statFactory)
            {
                _lazyStats = new Lazy<IEnumerable<IStat>>(() => new[] { statFactory() });
            }

            public IEnumerator<IStat> GetEnumerator() => _lazyStats.Value.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}