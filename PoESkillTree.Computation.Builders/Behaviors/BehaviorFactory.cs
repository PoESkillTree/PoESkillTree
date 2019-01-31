using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EnumsNET;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    public class BehaviorFactory
    {
        private readonly ConcurrentDictionary<CacheKey, Behavior> _cache =
            new ConcurrentDictionary<CacheKey, Behavior>();

        private readonly IStatFactory _statFactory;

        public BehaviorFactory(IStatFactory statFactory)
        {
            _statFactory = statFactory;
        }

        public IReadOnlyList<Behavior> ConvertTo(IStat source, IStat target) => new[]
        {
            ConversionTargetBase(source, target),
            ConversionTargetUncappedSubtotal(source, target),
            ConversionSourcePathTotal(source),
            ConvertToUncappedSubtotal(source, target)
        };

        public IReadOnlyList<Behavior> GainAs(IStat source, IStat target) => new[]
        {
            ConversionTargetBase(source, target),
            ConversionTargetUncappedSubtotal(source, target),
        };

        public IReadOnlyList<Behavior> SkillConversion(IStat source) => new[]
        {
            SkillConversionUncappedSubtotal(source)
        };

        private Behavior ConversionTargetBase(IStat source, IStat target) => GetOrAdd(
            target, NodeType.Base, BehaviorPathRules.ConversionWithSpecificSource(source),
            v => new ConversionTargetBaseValue(
                _statFactory.ConvertTo(source, target), _statFactory.GainAs(source, target), v),
            new CacheKey(source, target));

        private Behavior ConversionTargetUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            target, NodeType.UncappedSubtotal, BehaviorPathRules.All,
            v => new ConversionTargeUncappedSubtotalValue(source, target, v),
            new CacheKey(source, target));

        private Behavior ConversionSourcePathTotal(IStat source) => GetOrAdd(
            source, NodeType.PathTotal, BehaviorPathRules.All,
            v => new ConversionSourcePathTotalValue(_statFactory.Conversion(source), v),
            new CacheKey(source));

        private Behavior ConvertToUncappedSubtotal(IStat source, IStat target) => GetOrAdd(
            () => _statFactory.ConvertTo(source, target), NodeType.UncappedSubtotal, BehaviorPathRules.All,
            v => new ConvertToUncappedSubtotalValue(
                _statFactory.ConvertTo(source, target), _statFactory.Conversion(source),
                _statFactory.SkillConversion(source), v),
            new CacheKey(source, target));

        private Behavior SkillConversionUncappedSubtotal(IStat source) => GetOrAdd(
            () => _statFactory.SkillConversion(source), NodeType.UncappedSubtotal, BehaviorPathRules.All,
            v => new SkillConversionUncappedSubtotalValue(_statFactory.SkillConversion(source), v),
            new CacheKey(source));

        public IReadOnlyList<Behavior> Regen(Pool pool, Entity entity) =>
            new[] { RegenUncappedSubtotalBehavior(pool, entity) };

        private Behavior RegenUncappedSubtotalBehavior(Pool pool, Entity entity) => GetOrAdd(
            () => _statFactory.Regen(entity, pool), NodeType.UncappedSubtotal, BehaviorPathRules.All,
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
            => GetOrAdd(affectedStat, NodeType.BaseSet, BehaviorPathRules.All,
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
                NodeType.Base, BehaviorPathRules.NonConversion,
                v => new DamageEffectivenessBaseValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification),
                    _statFactory.DamageBaseSetEffectiveness(stat.Entity),
                    _statFactory.DamageBaseAddEffectiveness(stat.Entity), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageUncappedSubtotalBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(() => _statFactory.ConcretizeDamage(stat, damageSpecification),
                NodeType.UncappedSubtotal, BehaviorPathRules.All,
                v => new AilmentDamageUncappedSubtotalValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification),
                    _statFactory.ConcretizeDamage(stat, damageSpecification.ForSkills()), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageBaseBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(() => _statFactory.ConcretizeDamage(stat, damageSpecification),
                NodeType.Base, BehaviorPathRules.NonConversion,
                v => new AilmentDamageBaseValue(
                    _statFactory.ConcretizeDamage(stat, damageSpecification.ForSkills()), v),
                new CacheKey(stat, damageSpecification));

        private Behavior AilmentDamageIncreaseMoreBehavior(IStat stat, IDamageSpecification damageSpecification) =>
            GetOrAdd(new LazyStatList(() => _statFactory.ConcretizeDamage(stat, damageSpecification)),
                new[] { NodeType.Increase, NodeType.More }, BehaviorPathRules.All,
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
            return GetOrAdd(stat, nodeType, BehaviorPathRules.All,
                v => new AffectedByModifiersToOtherStatValue(stat, otherStat,
                    _statFactory.StatIsAffectedByModifiersToOtherStat(stat, otherStat, form), form, v),
                new CacheKey(stat, otherStat, form));
        }

        public IReadOnlyList<Behavior> Requirement(IStat stat)
            => new[] { RequirementBehavior(stat) };

        private Behavior RequirementBehavior(IStat stat)
            => GetOrAdd(() => _statFactory.Requirement(stat),
                NodeType.UncappedSubtotal, BehaviorPathRules.All,
                v => new RequirementUncappedSubtotalValue(_statFactory.Requirement(stat), v),
                new CacheKey(stat));

        public IReadOnlyList<Behavior> ItemProperty(IStat stat, ItemSlot slot)
            => new[] { ItemPropertyBehavior(stat, slot) };

        private Behavior ItemPropertyBehavior(IStat stat, ItemSlot slot)
            => GetOrAdd(() => _statFactory.ItemProperty(stat, slot),
                NodeType.Total, BehaviorPathRules.All,
                v => new RoundedValue(v, stat.DataType == typeof(double) ? 2 : 0),
                new CacheKey(stat, slot));

        private Behavior GetOrAdd(
            IStat affectedStat, NodeType affectNodeType, IBehaviorPathRule affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return GetOrAdd(new[] { affectedStat }, affectNodeType, affectedPaths, valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            Func<IStat> lazyAffectedStat, NodeType affectNodeType, IBehaviorPathRule affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return GetOrAdd(new LazyStatList(lazyAffectedStat), affectNodeType, affectedPaths,
                valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            IReadOnlyList<IStat> affectedStats, NodeType affectNodeType, IBehaviorPathRule affectedPaths,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return GetOrAdd(affectedStats, new[] { affectNodeType }, affectedPaths, valueTransformation, cacheKey);
        }

        private Behavior GetOrAdd(
            IReadOnlyList<IStat> affectedStats,
            IReadOnlyList<NodeType> affectNodeTypes, IBehaviorPathRule affectedPathsRule,
            Func<IValue, IValue> valueTransformation, CacheKey cacheKey)
        {
            return _cache.GetOrAdd(cacheKey, _ =>
                new Behavior(affectedStats, affectNodeTypes, affectedPathsRule,
                    new ValueTransformation(valueTransformation)));
        }

        private class CacheKey : ValueObject
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

            protected override object ToTuple() => (_behaviorName, WithSequenceEquality(_parameters));
        }

        private class LazyStatList : IReadOnlyList<IStat>
        {
            private readonly Lazy<IReadOnlyList<IStat>> _lazyStats;

            public LazyStatList(Func<IStat> statFactory)
            {
                _lazyStats = new Lazy<IReadOnlyList<IStat>>(() => new[] { statFactory() });
            }

            public IEnumerator<IStat> GetEnumerator() => _lazyStats.Value.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => 1;

            public IStat this[int index] => _lazyStats.Value[index];
        }
    }
}