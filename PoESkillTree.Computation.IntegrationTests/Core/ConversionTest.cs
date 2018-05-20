using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [TestFixture]
    public class ConversionTest
    {
        // This test implements and tests the behaviors necessary for stat conversions.
        // The behaviors will later be moved (and cleaned up, no Foo and Bar) to the Computation.Core project when
        // builder support for conversions is implemented.

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
            _sut = Calculator.CreateCalculator();
            _bar = new Stat("Bar");
            _foo = new Stat("Foo");
            _barFooConversion = new Stat("BarFooConversion");
            _barFooGain = new Stat("BarFooGain");
            _barConversion = new Stat("BarConversion");
            _barSkillConversion = new Stat("BarSkillConversion");
            var fooPathTotalBehavior = new Behavior(new[] { _foo }, new[] { NodeType.PathTotal },
                BehaviorPathInteraction.ConversionPathsOnly,
                new ValueTransformation(v => new FooPathTotalValue(_barFooConversion, _barFooGain, v)));
            var fooUncappedSubtotalBehavior = new Behavior(new[] { _foo }, new[] { NodeType.UncappedSubtotal },
                BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new FooUncappedSubtotalValue(_foo, _bar, v)));
            var barPathTotalBehavior = new Behavior(new[] { _bar }, new[] { NodeType.PathTotal },
                BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new BarPathTotalValue(_barConversion, v)));
            var barFooConversionUncappedSubtotalValue = new Behavior(new[] { _barFooConversion },
                new[] { NodeType.UncappedSubtotal }, BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v =>
                    new BarFooConversionUncappedSubtotalValue(_barFooConversion, _barConversion, _barSkillConversion,
                        v)));
            var barSkillConversionUncappedSubtotalBehavior = new Behavior(new[] { _barSkillConversion },
                new[] { NodeType.UncappedSubtotal }, BehaviorPathInteraction.AllPaths,
                new ValueTransformation(v => new BarSkillConversionUncappedSubtotalValue(_barSkillConversion, v)));
            ((Stat) _barFooConversion).Behaviors = new[]
            {
                fooPathTotalBehavior, fooUncappedSubtotalBehavior, barPathTotalBehavior,
                barFooConversionUncappedSubtotalValue
            };
            ((Stat) _barFooGain).Behaviors = new[]
            {
                fooPathTotalBehavior, fooUncappedSubtotalBehavior, barPathTotalBehavior
            };
            ((Stat) _barSkillConversion).Behaviors = new[]
            {
                barSkillConversionUncappedSubtotalBehavior
            };
        }

        [Test]
        public void SimpleGain()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(_barFooGain, Form.BaseAdd, 0.5)
                .DoUpdate();

            Assert.AreEqual(new NodeValue(1.5), GetValue(_foo));
        }

        [Test]
        public void SimpleConversion()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(new[] { _barFooConversion, _barConversion, _barSkillConversion }, Form.BaseAdd, 1.0 / 3)
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
                .AddModifier(barFooConversion, Form.BaseAdd, 0.5, skillSource)
                .AddModifier(barFooConversion, Form.BaseAdd, 0.3)
                .AddModifier(barFooConversion, Form.BaseAdd, 0.3)
                .AddModifier(_barFooGain, Form.BaseAdd, 0.2)
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
    }


    // Behavior of BarFooConversion and BarFooGain
    // Applies to Foo.PathTotal (conversion paths only)
    // Returns value * (BarFooConversion + BarFooGain)
    internal class FooPathTotalValue : IValue
    {
        private readonly IStat _barFooConversion;
        private readonly IStat _barFooGain;
        private readonly IValue _transformedValue;

        public FooPathTotalValue(IStat barFooConversion, IStat barFooGain, IValue transformedValue)
        {
            _barFooConversion = barFooConversion;
            _barFooGain = barFooGain;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            if (value is null)
                return null;

            var conversion = context.GetValue(_barFooConversion) ?? new NodeValue(0);
            var gain = context.GetValue(_barFooGain) ?? new NodeValue(0);
            return value * (conversion + gain);
        }
    }


    // Behavior of BarFooConversion and BarFooGain
    // Applies to Foo.UncappedSubtotal
    // Modifies the context to append the missing conversion paths from Bar when querying Foo's paths
    internal class FooUncappedSubtotalValue : IValue
    {
        private readonly IStat _foo;
        private readonly IStat _bar;
        private readonly IValue _transformedValue;

        public FooUncappedSubtotalValue(IStat foo, IStat bar, IValue transformedValue)
        {
            _foo = foo;
            _bar = bar;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedContext(_foo, _bar, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _foo;
            private readonly IStat _bar;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat foo, IStat bar, IValueCalculationContext originalContext)
            {
                _foo = foo;
                _bar = bar;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat)
            {
                if (!_foo.Equals(stat))
                    return _originalContext.GetPaths(stat);

                var originalPaths = _originalContext.GetPaths(_foo);
                var conversionPaths = _originalContext.GetPaths(_bar)
                    .Select(p => new PathDefinition(p.ModifierSource, _bar.Concat(p.ConversionStats).ToList()));
                return originalPaths.Concat(conversionPaths).Distinct();
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(
                Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }


    // Behavior of Bar*Conversion and Bar*Gain
    // Applies to Bar.PathTotal (all paths)
    // Returns value * (1 - BarConversion).Clip(0, 1)
    internal class BarPathTotalValue : IValue
    {
        private readonly IStat _barConversion;
        private readonly IValue _transformedValue;

        public BarPathTotalValue(IStat barConversion, IValue transformedValue)
        {
            _barConversion = barConversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            if (value is null)
                return null;

            var conversion = context.GetValue(_barConversion) ?? new NodeValue(0);
            return value * (1 - conversion).Clip(0, 1);
        }
    }


    // Behavior of BarFooConversion
    // Applies to BarFooConversion.UncappedSubtotal
    // Modifies the context to apply multipliers to BarFooConversion.PathTotal nodes (see code for details)
    internal class BarFooConversionUncappedSubtotalValue : IValue
    {
        private readonly IStat _barFooConversion;
        private readonly IStat _barConversion;
        private readonly IStat _barSkillConversion;
        private readonly IValue _transformedValue;

        public BarFooConversionUncappedSubtotalValue(
            IStat barFooConversion, IStat barConversion, IStat barSkillConversion, IValue transformedValue)
        {
            _barFooConversion = barFooConversion;
            _barConversion = barConversion;
            _barSkillConversion = barSkillConversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext =
                new ModifiedContext(_barFooConversion, _barConversion, _barSkillConversion, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _barFooConversion;
            private readonly IStat _barConversion;
            private readonly IStat _barSkillConversion;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat barFooConversion, IStat barConversion, IStat barSkillConversion,
                IValueCalculationContext originalContext)
            {
                _barFooConversion = barFooConversion;
                _barConversion = barConversion;
                _barSkillConversion = barSkillConversion;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat) =>
                _originalContext.GetPaths(stat);

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path)
            {
                var value = _originalContext.GetValue(stat, nodeType, path);
                if (value is null || !_barFooConversion.Equals(stat) || nodeType != NodeType.PathTotal)
                    return value;

                var barConversion = _originalContext.GetValue(_barConversion) ?? new NodeValue(0);
                if (barConversion <= 1)
                {
                    // Conversions don't exceed 100%, No scaling required
                    return value;
                }

                var isSkillPath = path.ModifierSource is ModifierSource.Local.Skill;
                var barSkillConversion = _originalContext.GetValue(_barSkillConversion) ?? new NodeValue(0);
                if (barSkillConversion >= 1)
                {
                    // Conversions from skills are or exceed 100%
                    // Non-skill conversions don't apply
                    if (!isSkillPath)
                        return new NodeValue(0);
                    // Skill conversions are scaled to sum to 100%
                    return value / barSkillConversion;
                }

                // Conversions exceed 100%
                // Skill conversions don't scale (they themselves don't exceed 100%)
                if (isSkillPath)
                    return value;
                // Non-skill conversions are scaled to sum to 100% - skill conversions
                return value * (1 - barSkillConversion) / (barConversion - barSkillConversion);
            }

            public IEnumerable<NodeValue?> GetValues(
                Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }


    // Behavior of BarSkillConversion
    // Applies to BarSkillConversion.UncappedSubtotal
    // Modifies the context to only return path with skill sources
    internal class BarSkillConversionUncappedSubtotalValue : IValue
    {
        private readonly IStat _barSkillConversion;
        private readonly IValue _transformedValue;

        public BarSkillConversionUncappedSubtotalValue(IStat barSkillConversion, IValue transformedValue)
        {
            _barSkillConversion = barSkillConversion;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedContext(_barSkillConversion, context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly IStat _barSkillConversion;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(IStat barSkillConversion, IValueCalculationContext originalContext)
            {
                _barSkillConversion = barSkillConversion;
                _originalContext = originalContext;
            }

            public IEnumerable<PathDefinition> GetPaths(IStat stat)
            {
                if (!_barSkillConversion.Equals(stat))
                    return _originalContext.GetPaths(stat);

                return _originalContext.GetPaths(stat)
                    .Where(p => p.ModifierSource is ModifierSource.Local.Skill);
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                _originalContext.GetValue(stat, nodeType, path);

            public IEnumerable<NodeValue?> GetValues(
                Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                _originalContext.GetValues(form, paths);
        }
    }
}