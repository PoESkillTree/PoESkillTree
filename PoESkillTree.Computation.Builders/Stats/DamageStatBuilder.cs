using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
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

        protected override IStat BuildKeywordStat(IDamageSpecification spec, Entity entity, Keyword keyword)
        {
            return spec.Ailment.HasValue
                ? StatFactory.MainSkillPartAilmentDamageHasKeyword(entity, keyword)
                : StatFactory.MainSkillPartDamageHasKeyword(entity, keyword, spec.DamageSource);
        }
    }
}