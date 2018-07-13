using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Buffs
{
    public class BuffBuilderCollection : IBuffBuilderCollection
    {
        private readonly IStatFactory _statFactory;
        private readonly IReadOnlyList<BuffBuilderWithKeywords> _buffs;
        private readonly BuffRestrictionsBuilder _restrictionsBuilder;
        private readonly IEntityBuilder _source;
        private readonly IEntityBuilder _target;

        public BuffBuilderCollection(
            IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs,
            IEntityBuilder source, IEntityBuilder target)
            : this(statFactory, buffs, new BuffRestrictionsBuilder(), source, target)
        {
        }

        private BuffBuilderCollection(
            IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs,
            BuffRestrictionsBuilder restrictionsBuilder, IEntityBuilder source, IEntityBuilder target)
        {
            _statFactory = statFactory;
            _buffs = buffs;
            _restrictionsBuilder = restrictionsBuilder;
            _source = source;
            _target = target;
        }

        public IBuilderCollection Resolve(ResolveContext context)
        {
            var buffs = _buffs.Select(b => b.Resolve(context)).ToList();
            return new BuffBuilderCollection(_statFactory, buffs, _restrictionsBuilder.Resolve(context),
                _source.Resolve(context), _target.Resolve(context));
        }

        public ValueBuilder Count()
        {
            return new ValueBuilder(new ValueBuilderImpl(Build, c => Resolve(c).Count()));

            IValue Build(BuildParameters parameters) =>
                new CountingValue(CreateValues(parameters).ToList());
        }

        public IConditionBuilder Any() => Count() > 0;

        private IEnumerable<IValue> CreateValues(BuildParameters parameters)
        {
            var restrictions = _restrictionsBuilder.Build();
            var sourceEntities = _source.Build(parameters.ModifierSourceEntity);
            var targetEntities = _target.Build(parameters.ModifierSourceEntity);
            return from b in _buffs
                   where restrictions.AllowsBuff(b)
                   let buffIdentity = b.Buff.Build()
                   from t in targetEntities
                   let activeStat = _statFactory.BuffIsActive(t, buffIdentity)
                   let activeCondition = new StatValue(activeStat)
                   let buffSourceCondition = BuffSourceIsAny(t, buffIdentity, sourceEntities)
                   select new ConditionalValue(
                       c => activeCondition.Calculate(c).IsTrue() && buffSourceCondition.Calculate(c).IsTrue(),
                       $"{activeCondition} && {buffSourceCondition}");
        }

        private IValue BuffSourceIsAny(Entity target, string buffIdentity, IEnumerable<Entity> sources)
        {
            var statValues = sources
                .Select(s => _statFactory.BuffSourceIs(target, buffIdentity, s))
                .Select(s => new StatValue(s))
                .ToList();
            var count = new CountingValue(statValues);
            return new ConditionalValue(c => count.Calculate(c) > 0, $"{count} > 0");
        }

        public IStatBuilder Effect =>
            new StatBuilder(_statFactory, new BuffCoreStatBuilder(_buffs, b => b.Effect, _restrictionsBuilder))
                .For(_source);

        public IStatBuilder AddStat(IStatBuilder stat) =>
            new StatBuilder(_statFactory, new BuffCoreStatBuilder(_buffs, b => b.AddStat(stat), _restrictionsBuilder))
                .For(_source);

        public IFlagStatBuilder ApplyToEntity(IEntityBuilder target)
        {
            var coreStats = _buffs
                .Select(b => ApplyToEntity(b.Buff, target))
                .Select(b => new StatBuilderAdapter(b))
                .ToList();
            return new StatBuilder(_statFactory, new ConcatCompositeCoreStatBuilder(coreStats));
        }

        private IStatBuilder ApplyToEntity(IBuffBuilder buff, IEntityBuilder target) =>
            buff.On(target).WithCondition(buff.IsOn(_source, _target));

        public IBuffBuilderCollection With(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.With(keyword), _source, _target);

        public IBuffBuilderCollection Without(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.Without(keyword), _source, _target);
    }
}