using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Charges
{
    public class ChargeTypeBuilder : StatBuildersBase, IChargeTypeBuilder
    {
        private readonly ChargeType _chargeType;

        public ChargeTypeBuilder(IStatFactory statFactory, ChargeType chargeType) : base(statFactory)
        {
            _chargeType = chargeType;
        }

        public IChargeTypeBuilder Resolve(ResolveContext context) => this;

        public IStatBuilder Amount => FromIdentity($"{_chargeType}.Amount", typeof(int));
        public IStatBuilder Duration => FromIdentity($"{_chargeType}.Duration", typeof(double));
        public IStatBuilder ChanceToGain => FromIdentity($"{_chargeType}.ChanceToGain", typeof(int));

        public IActionBuilder GainAction => new ActionBuilder(StatFactory,
            CoreBuilder.Create($"{_chargeType}.GainAction"), new ModifierSourceEntityBuilder());
    }
}