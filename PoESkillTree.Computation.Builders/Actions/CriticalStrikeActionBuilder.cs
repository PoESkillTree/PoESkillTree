using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Actions
{
    internal class CriticalStrikeActionBuilder : ActionBuilder, ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilder(IStatFactory statFactory, IEntityBuilder entity)
            : base(statFactory, "CriticalStrike", entity)
        {
        }

        public IDamageRelatedStatBuilder Chance =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, $"{Identity}.Chance", typeof(double))
                .WithHits;

        public IDamageRelatedStatBuilder Multiplier =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, $"{Identity}.Multiplier", typeof(double))
                .WithHitsAndAilments;

        public IStatBuilder ExtraDamageTaken =>
            StatBuilderUtils.FromIdentity(StatFactory, $"{Identity}.ExtraDamageTaken", typeof(int));
    }
}