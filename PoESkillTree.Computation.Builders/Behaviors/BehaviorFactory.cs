using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class BehaviorFactory
    {
        private readonly IDictionary<string, Behavior> _cache = new Dictionary<string, Behavior>();
        private readonly IStatFactory _statFactory;

        public BehaviorFactory(IStatFactory statFactory)
        {
            _statFactory = statFactory;
        }

        public IReadOnlyList<Behavior> ConvertTo(IStat source, IStat target) => new[]
        {
            ConversionTargetPathTotal(source, target),
            ConversionTargeUncappedSubtotal(source, target),
            ConversionSourcePathTotal(source),
            ConvertToUncappedSubtotal(source, target)
        };

        public IReadOnlyList<Behavior> GainAs(IStat source, IStat target) => new[]
        {
            ConversionTargetPathTotal(source, target),
            ConversionTargeUncappedSubtotal(source, target),
        };

        public IReadOnlyList<Behavior> SkillConversion(IStat source) => new[]
        {
            SkillConversionUncappedSubtotal(source)
        };

        private Behavior ConversionTargetPathTotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.PathTotal, BehaviorPathInteraction.ConversionPathsOnly,
            v => new ConversionTargetPathTotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.GainAs(source, target), v));

        private Behavior ConversionTargeUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new ConversionTargeUncappedSubtotalValue(source, target, v));

        private Behavior ConversionSourcePathTotal(IStat source) => GetOrAdd(
            source, NodeType.PathTotal, BehaviorPathInteraction.AllPaths,
            v => new ConversionSourcePathTotalValue(_statFactory.Conversion(source), v));

        private Behavior ConvertToUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            () => _statFactory.ConvertTo(source, target), NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new ConvertToUncappedSubtotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.Conversion(source),
                _statFactory.SkillConversion(source), v));

        private Behavior SkillConversionUncappedSubtotal(IStat source) => GetOrAdd(
            () => _statFactory.SkillConversion(source), NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new SkillConversionUncappedSubtotalValue(_statFactory.SkillConversion(source), v));

        private Behavior GetOrAdd(
            IStat affectedStat, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, [CallerMemberName] string behaviorName = null)
        {
            return GetOrAdd(new[] { affectedStat }, affectNodeType, affectedPaths, valueTransformation,
                behaviorName);
        }

        private Behavior GetOrAdd(
            Func<IStat> lazyAffectedStat, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, [CallerMemberName] string behaviorName = null)
        {
            return GetOrAdd(new LazyStatEnumerable(lazyAffectedStat), affectNodeType, affectedPaths,
                valueTransformation, behaviorName);
        }

        private Behavior GetOrAdd(
            IEnumerable<IStat> affectedStats, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, [CallerMemberName] string behaviorName = null)
        {
            return _cache.GetOrAdd(behaviorName, _ =>
                new Behavior(affectedStats, new[] { affectNodeType }, affectedPaths,
                    new ValueTransformation(valueTransformation)));
        }

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