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

        public IReadOnlyList<(string regex, IChargeTypeProvider match)> Matchers { get; }

        private MatcherCollection<IChargeTypeProvider> CreateCollection() =>
            new MatcherCollection<IChargeTypeProvider>
            {
                { "endurance charges?", Charge.Endurance },
                { "power charges?", Charge.Power },
                { "endurance charges?", Charge.Endurance },
            };
    }
}