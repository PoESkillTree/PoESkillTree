using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Charges;

namespace PoESkillTree.Computation.Data
{
    public class ChargeTypeMatchers : IReferencedMatchers<IChargeTypeProvider>
    {
        private IChargeTypeProviderFactory Charge { get; }

        public ChargeTypeMatchers(IChargeTypeProviderFactory chargeTypeProviderFactory)
        {
            Charge = chargeTypeProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        public string ReferenceName { get; } = nameof(ChargeTypeMatchers);

        public IReadOnlyList<ReferencedMatcherData<IChargeTypeProvider>> Matchers { get; }

        private ReferencedMatcherCollection<IChargeTypeProvider> CreateCollection() =>
            new ReferencedMatcherCollection<IChargeTypeProvider>
            {
                { "endurance charges?", Charge.Endurance },
                { "power charges?", Charge.Power },
                { "endurance charges?", Charge.Endurance },
            };
    }
}