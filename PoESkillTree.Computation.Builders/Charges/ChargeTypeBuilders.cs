using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ChargeTypeBuilders : IChargeTypeBuilders
    {
        public ChargeTypeBuilders(IStatFactory statFactory)
        {
            Endurance = new ChargeTypeBuilder(statFactory, CoreBuilder.Create(ChargeType.Endurance));
            Frenzy = new ChargeTypeBuilder(statFactory, CoreBuilder.Create(ChargeType.Frenzy));
            Power = new ChargeTypeBuilder(statFactory, CoreBuilder.Create(ChargeType.Power));
            ChanceToSteal =
                StatBuilderUtils.DamageRelatedFromIdentity(statFactory, "ChanceToStealACharge", typeof(int)).WithHits;
            Rage = new ChargeTypeBuilder(statFactory, CoreBuilder.Create(ChargeType.Rage));
            RageEffect = StatBuilderUtils.FromIdentity(statFactory, "RageEffect", typeof(double));
        }

        public IChargeTypeBuilder Endurance { get; }
        public IChargeTypeBuilder Frenzy { get; }
        public IChargeTypeBuilder Power { get; }

        public IDamageRelatedStatBuilder ChanceToSteal { get; }

        public IChargeTypeBuilder Rage { get; }
        public IStatBuilder RageEffect { get; }
    }
}