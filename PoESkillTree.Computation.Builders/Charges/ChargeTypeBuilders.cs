using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Charges;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ChargeTypeBuilders : IChargeTypeBuilders
    {
        public ChargeTypeBuilders(IStatFactory statFactory)
        {
            Endurance = new ChargeTypeBuilder(statFactory, ChargeType.Endurance);
            Frenzy = new ChargeTypeBuilder(statFactory, ChargeType.Frenzy);
            Power = new ChargeTypeBuilder(statFactory, ChargeType.Power);
        }

        public IChargeTypeBuilder Endurance { get; }
        public IChargeTypeBuilder Frenzy { get; }
        public IChargeTypeBuilder Power { get; }
    }
}