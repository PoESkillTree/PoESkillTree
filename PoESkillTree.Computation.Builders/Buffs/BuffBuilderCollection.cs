using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Conditions;
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

        public BuffBuilderCollection(IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs)
            : this(statFactory, buffs, new BuffRestrictionsBuilder())
        {
        }

        private BuffBuilderCollection(
            IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs,
            BuffRestrictionsBuilder restrictionsBuilder)
        {
            _statFactory = statFactory;
            _buffs = buffs;
            _restrictionsBuilder = restrictionsBuilder;
        }

        public IBuilderCollection Resolve(ResolveContext context)
        {
            var buffs = _buffs.Select(b => b.Resolve(context)).ToList();
            return new BuffBuilderCollection(_statFactory, buffs, _restrictionsBuilder.Resolve(context));
        }

        public ValueBuilder Count()
        {
            return new ValueBuilder(new ValueBuilderImpl(Build, c => Resolve(c).Count()));

            IValue Build(BuildParameters parameters)
            {
                var values = CreateConditions()
                    .Select(condition => condition.Build(parameters).Value);
                return new CountingValue(values.ToList());
            }
        }

        public IConditionBuilder Any()
        {
            return new ValueConditionBuilder(Build, c => Resolve(c).Any());

            IValue Build(BuildParameters parameters) =>
                CreateConditions()
                    .Aggregate((l, r) => l.Or(r))
                    .Build(parameters).Value;
        }

        private IEnumerable<IConditionBuilder> CreateConditions()
        {
            var restrictions = _restrictionsBuilder.Build();
            return from b in _buffs
                   let buffCondition = b.Buff.IsOn(new ModifierSourceEntityBuilder())
                   let allowedCondition = ConstantConditionBuilder.Create(restrictions.AllowsBuff(b))
                   select allowedCondition.And(buffCondition);
        }

        public IStatBuilder Effect =>
            new StatBuilder(_statFactory, new BuffCoreStatBuilder(_buffs, b => b.Effect, _restrictionsBuilder));

        public IStatBuilder AddStat(IStatBuilder stat) =>
            new StatBuilder(_statFactory, new BuffCoreStatBuilder(_buffs, b => b.AddStat(stat), _restrictionsBuilder));

        public IBuffBuilderCollection With(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.With(keyword));

        public IBuffBuilderCollection Without(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.Without(keyword));
    }
}