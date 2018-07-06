using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Actions
{
    internal class CriticalStrikeActionBuilder : ActionBuilder, ICriticalStrikeActionBuilder
    {
        public CriticalStrikeActionBuilder(IStatFactory statFactory, IEntityBuilder entity)
            : base(statFactory, new ConstantStringBuilder("CriticalStrike"), entity)
        {
        }

        public IDamageRelatedStatBuilder Chance =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, $"{BuildIdentity()}.Chance", typeof(double))
                .WithHits;

        public IDamageRelatedStatBuilder Multiplier =>
            StatBuilderUtils.DamageRelatedFromIdentity(StatFactory, $"{BuildIdentity()}.Multiplier", typeof(double))
                .WithHitsAndAilments;

        public IStatBuilder ExtraDamageTaken =>
            StatBuilderUtils.FromIdentity(StatFactory, $"{BuildIdentity()}.ExtraDamageTaken", typeof(int));
    }
}