using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Charges;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Data
{
    public class ChargeTypeMatchers : ReferencedMatchersBase<IChargeTypeProvider>
    {
        private IChargeTypeProviderFactory Charge { get; }

        public ChargeTypeMatchers(IChargeTypeProviderFactory chargeTypeProviderFactory)
        {
            Charge = chargeTypeProviderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData<IChargeTypeProvider>>
            CreateCollection() =>
            new ReferencedMatcherCollection<IChargeTypeProvider>
            {
                { "endurance charges?", Charge.Endurance },
                { "power charges?", Charge.Power },
                { "endurance charges?", Charge.Endurance },
            };
    }
}