using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ChargeTypeBuilders : IChargeTypeBuilders
    {
        private readonly IStatFactory _statFactory;

        public ChargeTypeBuilders(IStatFactory statFactory)
        {
            _statFactory = statFactory;
            Endurance = From(ChargeType.Endurance);
            Frenzy = From(ChargeType.Frenzy);
            Power = From(ChargeType.Power);
            ChanceToSteal =
                StatBuilderUtils.DamageRelatedFromIdentity(statFactory, "ChanceToStealACharge", typeof(uint)).WithHits;
            Rage = From(ChargeType.Rage);
            RageEffect = StatBuilderUtils.FromIdentity(statFactory, "RageEffect", typeof(double));
        }

        public IChargeTypeBuilder Endurance { get; }
        public IChargeTypeBuilder Frenzy { get; }
        public IChargeTypeBuilder Power { get; }

        public IDamageRelatedStatBuilder ChanceToSteal { get; }

        public IChargeTypeBuilder Rage { get; }
        public IStatBuilder RageEffect { get; }

        public IChargeTypeBuilder From(ChargeType type)
            => new ChargeTypeBuilder(_statFactory, CoreBuilder.Create(type));
    }
}