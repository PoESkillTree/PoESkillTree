using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class CastRateStatBuilder : DamageRelatedStatBuilder
    {
        public CastRateStatBuilder(IStatFactory statFactory)
            : this(statFactory,
                LeafCoreStatBuilder.FromIdentity(statFactory, "CastRate", typeof(double)),
                new DamageStatConcretizer(statFactory, new DamageSpecificationBuilder()).WithHits(),
                s => new[] { s })
        {
        }

        private CastRateStatBuilder(
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
            new CastRateStatBuilder(StatFactory, coreStatBuilder, statConcretizer, statConverter);

        protected override IStat BuildKeywordStat(IDamageSpecification spec, Entity entity, Keyword keyword)
            => StatFactory.MainSkillPartCastRateHasKeyword(entity, keyword);
    }
}