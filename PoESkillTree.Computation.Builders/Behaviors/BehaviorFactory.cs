using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EnumsNET;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Utils.Extensions;

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
            ConversionTargetUncappedSubtotal(source, target),
            ConversionSourcePathTotal(source),
            ConvertToUncappedSubtotal(source, target)
        };

        public IReadOnlyList<Behavior> GainAs(IStat source, IStat target) => new[]
        {
            ConversionTargetPathTotal(source, target),
            ConversionTargetUncappedSubtotal(source, target),
        };

        public IReadOnlyList<Behavior> SkillConversion(IStat source) => new[]
        {
            SkillConversionUncappedSubtotal(source)
        };

        private Behavior ConversionTargetPathTotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.PathTotal, BehaviorPathInteraction.Conversion,
            v => new ConversionTargetPathTotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.GainAs(source, target), v),
            new CacheKey(source, target));

        private Behavior ConversionTargetUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
            v => new ConversionTargeUncappedSubtotalValue(source, target, v),
            new CacheKey(source, target));

        private Behavior ConversionSourcePathTotal(IStat source) => GetOrAdd(
            source, NodeType.PathTotal, BehaviorPathInteraction.All,
            v => new ConversionSourcePathTotalValue(_statFactory.Conversion(source), v),
            new CacheKey(source));

        private Behavior ConvertToUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            () => _statFactory.ConvertTo(source, target), NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
            v => new ConvertToUncappedSubtotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.Conversion(source),
                _statFactory.SkillConversion(source), v),
            new CacheKey(source, target));

        private Behavior SkillConversionUncappedSubtotal(IStat source) => GetOrAdd(
            () => _statFactory.SkillConversion(source), NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
            v => new SkillConversionUncappedSubtotalValue(_statFactory.SkillConversion(source), v),
            new CacheKey(source));

        public IReadOnlyList<Behavior> Regen(Pool pool, Entity entity) =>
            new[] { RegenUncappedSubtotalBehavior(pool, entity) };

        private Behavior RegenUncappedSubtotalBehavior(Pool pool, Entity entity) => GetOrAdd(
            () => _statFactory.Regen(entity, pool), NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
            v => new RegenUncappedSubtotalValue(
                pool, p => _statFactory.Regen(entity, p), p => _statFactory.RegenTargetPool(entity, p), v),
            new CacheKey(pool, entity));

        public IReadOnlyList<Behavior> ActiveSkillItemSlot(Entity entity, string skillId)
            => new[] { ActiveSkillItemSlotBaseSetByMaximumBehavior(entity, skillId) };

        private Behavior ActiveSkillItemSlotBaseSetByMaximumBehavior(Entity entity, string skillId)
            => BaseSetByMaximumBehavior(
                () => _statFactory.ActiveSkillItemSlot(entity, skillId),
                new CacheKey(entity, skillId));

        public IReadOnlyList<Behavior> ActiveSkillSocketIndex(Entity entity, string skillId)
            => new[] { ActiveSkillSocketIndexBaseSetByMaximumBehavior(entity, skillId) };

        private Behavior ActiveSkillSocketIndexBaseSetByMaximumBehavior(Entity entity, string skillId)
            => BaseSetByMaximumBehavior(
                () => _statFactory.ActiveSkillSocketIndex(entity, skillId),
                new CacheKey(entity, skillId));

        private Behavior BaseSetByMaximumBehavior(Func<IStat> affectedStat, CacheKey cacheKey)
            => GetOrAdd(affectedStat, NodeType.BaseSet, BehaviorPathInteraction.All,
                v => new MaximumFormAggregatingValue(affectedStat(), Form.BaseSet, v),
                cacheKey);

        public IReadOnlyList<Behavior> ConcretizeDamage(IStat stat, IDamageSpecification damageSpecification)
        {
            // Behaviors are only for damage, not other damage related stats
            if (!Enums.GetValues<DamageType>().Any(t => _statFactory.Damage(stat.Entity, t).Equals(stat)))
                return new Behavior[0];

            if (damageSpecification.DamageSource == DamageSource.OverTime)
            {
                // Skill DoT
                return new Behavior[0];
            }
            else if (damageSpecification.IsSkillDamage())
            {
                // Skill Hit
                return new[]
                {
                    DamageEffectivenessBaseBehavior(stat, damageSpecification)
                };
            }
            else
            {
                // Ailment
                return new[]
                {
                    AilmentDamageUncappedSubtotalBehavior(stat, damageSpecification),
                    AilmentDamageBaseBehavior(stat, damageSpecification),
                    AilmentDamageIncreaseMoreBehavior(stat, damageSpecification)
                };
            }
        }

        private Behavior DamageEffectivenessBaseBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(() => _statFactory.ConcretizeDamage(stat, damageSpecification),
                NodeType.Base, BehaviorPathInteraction.NonConversion,
                v => new DamageEffectivenessBaseValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification),
                    _statFactory.DamageBaseSetEffectiveness(stat.Entity),
                    _statFactory.DamageBaseAddEffectiveness(stat.Entity), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageUncappedSubtotalBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(() => _statFactory.ConcretizeDamage(stat, damageSpecification),
                NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
                v => new AilmentDamageUncappedSubtotalValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification),
                    _statFactory.ConcretizeDamage(stat, damageSpecification.ForSkills()), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageBaseBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(() => _statFactory.ConcretizeDamage(stat, damageSpecification),
                NodeType.Base, BehaviorPathInteraction.NonConversion,
                v => new AilmentDamageBaseValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification.ForSkills()), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageIncreaseMoreBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(new LazyStatEnumerable(() => _statFactory.ConcretizeDamage(stat, damageSpecification)),
                new[] { NodeType.Increase, NodeType.More }, BehaviorPathInteraction.All,
                v => new AilmentDamageIncreaseMoreValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification),
                    _statFactory.AilmentDealtDamageType(stat.Entity, damageSpecification.Ailment.Value),
                    t => _statFactory.ConcretizeDamage(_statFactory.Damage(stat.Entity, t), damageSpecification), v),
                new CacheKey(stat, damageSpecification));

        public IReadOnlyList<Behavior> StatIsAffectedByModifiersToOtherStat(IStat stat, IStat otherStat, Form form)
            => new[] { StatIsAffectedByModifiersToOtherStatBehavior(stat, otherStat, form) };

        private Behavior StatIsAffectedByModifiersToOtherStatBehavior(IStat stat, IStat otherStat, Form form)
        {
            var nodeType = Enums.Parse<NodeType>(form.ToString());
            return GetOrAdd(stat, nodeType, BehaviorPathInteraction.All,
                v => new AffectedByModifiersToOtherStatValue(stat, otherStat,
                    _statFactory.StatIsAffectedByModifiersToOtherStat(stat, otherStat, form), form, v),
                new CacheKey(stat, otherStat, form));
        }

        public IReadOnlyList<Behavior> Requirement(IStat stat)
            => new[] { RequirementBehavior(stat) };

        private Behavior RequirementBehavior(IStat stat)
            => GetOrAdd(() => _statFactory.Requirement(stat),
                NodeType.UncappedSubtotal, BehaviorPathInteraction.All,
                v => new RequirementUncappedSubtotalValue(_statFactory.Requirement(stat), v),
                new CacheKey(stat));

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
            return GetOrAdd(affectedStats, new[] { affectNodeType }, affectedPaths, valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            IEnumerable<IStat> affectedStats,
            IEnumerable<NodeType> affectNodeTypes, BehaviorPathInteraction affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return _cache.GetOrAdd(cacheKey, _ =>
                new Behavior(affectedStats, affectNodeTypes, affectedPaths,
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

            public CacheKey(
                object parameter1, object parameter2, object parameter3, [CallerMemberName] string behaviorName = null)
                : this(behaviorName, parameter1, parameter2, parameter3)
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