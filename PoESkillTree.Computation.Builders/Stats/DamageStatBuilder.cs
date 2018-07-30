using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public sealed class DamageStatBuilder : DamageRelatedStatBuilder, IDamageStatBuilder
    {
        public DamageStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
            : this(statFactory, coreStatBuilder,
                new DamageStatConcretizer(statFactory, new DamageSpecificationBuilder(), canApplyToSkillDamage: true),
                s => new[] { s })
        {
        }

        private DamageStatBuilder(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder,
            DamageStatConcretizer statConcretizer,
            Func<IStat, IEnumerable<IStat>> statConverter)
            : base(statFactory, coreStatBuilder, statConcretizer, statConverter)
        {
        }

        protected override DamageRelatedStatBuilder Create(
            ICoreStatBuilder coreStatBuilder,
            DamageStatConcretizer statConcretizer,
            Func<IStat, IEnumerable<IStat>> statConverter) =>
            new DamageStatBuilder(StatFactory, coreStatBuilder, statConcretizer, statConverter);

        public new IDamageStatBuilder For(IEntityBuilder entity) =>
            (IDamageStatBuilder) base.For(entity);

        public IDamageRelatedStatBuilder Taken =>
            ((IDamageRelatedStatBuilder) WithStatConverter(StatFactory.DamageTaken));

        public override IStatBuilder With(IKeywordBuilder keyword) =>
            With(StatConcretizer.With(spec => KeywordCondition(spec, keyword)));

        public override IStatBuilder NotWith(IKeywordBuilder keyword) =>
            With(StatConcretizer.With(spec => KeywordCondition(spec, keyword).Not));

        private IConditionBuilder KeywordCondition(IDamageSpecification spec, IKeywordBuilder keyword) =>
            ValueConditionBuilder.Create((ps, k) => BuildKeywordStat(spec, ps, k), keyword);

        private IStat BuildKeywordStat(IDamageSpecification spec, BuildParameters parameters, IKeywordBuilder keyword)
        {
            return StatFactory.ActiveSkillPartDamageHasKeyword(parameters.ModifierSourceEntity, keyword.Build(),
                spec.DamageSource);
        }
    }
}