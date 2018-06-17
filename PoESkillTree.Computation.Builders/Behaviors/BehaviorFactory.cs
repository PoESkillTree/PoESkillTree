using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class BehaviorFactory
    {
        private readonly IDictionary<CacheKey, Behavior> _cache = new Dictionary<CacheKey, Behavior>();
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
                _statFactory.ConvertTo(source, target), _statFactory.GainAs(source, target), v),
            new CacheKey(source, target));

        private Behavior ConversionTargeUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new ConversionTargeUncappedSubtotalValue(source, target, v),
            new CacheKey(source, target));

        private Behavior ConversionSourcePathTotal(IStat source) => GetOrAdd(
            source, NodeType.PathTotal, BehaviorPathInteraction.AllPaths,
            v => new ConversionSourcePathTotalValue(_statFactory.Conversion(source), v),
            new CacheKey(source));

        private Behavior ConvertToUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            () => _statFactory.ConvertTo(source, target), NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new ConvertToUncappedSubtotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.Conversion(source),
                _statFactory.SkillConversion(source), v),
            new CacheKey(source, target));

        private Behavior SkillConversionUncappedSubtotal(IStat source) => GetOrAdd(
            () => _statFactory.SkillConversion(source), NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new SkillConversionUncappedSubtotalValue(_statFactory.SkillConversion(source), v),
            new CacheKey(source));

        public IReadOnlyList<Behavior> Regen(Pool pool, Entity entity) =>
            new[] { RegenUncappedSubtotalBehavor(pool, entity) };

        private Behavior RegenUncappedSubtotalBehavor(Pool pool, Entity entity) => GetOrAdd(
            () => _statFactory.Regen(pool, entity), NodeType.UncappedSubtotal, BehaviorPathInteraction.AllPaths,
            v => new RegenUncappedSubtotalValue(
                pool, p => _statFactory.Regen(p, entity), p => _statFactory.RegenTargetPool(p, entity), v),
            new CacheKey(pool, entity));

        private Behavior GetOrAdd(
            IStat affectedStat, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return GetOrAdd(new[] { affectedStat }, affectNodeType, affectedPaths, valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            Func<IStat> lazyAffectedStat, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return GetOrAdd(new LazyStatEnumerable(lazyAffectedStat), affectNodeType, affectedPaths,
                valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            IEnumerable<IStat> affectedStats, NodeType affectNodeType, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return _cache.GetOrAdd(cacheKey, _ =>
                new Behavior(affectedStats, new[] { affectNodeType }, affectedPaths,
                    new ValueTransformation(valueTransformation)));
        }

        private struct CacheKey
        {
            private readonly string _behaviorName;
            private readonly IReadOnlyList<object> _parameters;

            public CacheKey(object parameter, [CallerMemberName] string behaviorName = null)
                : this(behaviorName, parameter)
            {
            }

            public CacheKey(object parameter1, object parameter2, [CallerMemberName] string behaviorName = null)
                : this(behaviorName, parameter1, parameter2)
            {
            }

            private CacheKey(string behaviorName, params object[] parameters)
            {
                _behaviorName = behaviorName;
                _parameters = parameters;
            }

            public override bool Equals(object obj) =>
                obj is CacheKey other && _behaviorName == other._behaviorName &&
                _parameters.SequenceEqual(other._parameters);

            public override int GetHashCode() =>
                (_behaviorName, _parameters.SequenceHash()).GetHashCode();
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