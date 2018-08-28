using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IChargeTypeBuilder"/>s.
    /// </summary>
    public class ChargeTypeMatchers : ReferencedMatchersBase<IChargeTypeBuilder>
    {
        private IChargeTypeBuilders Charge { get; }

        public ChargeTypeMatchers(IChargeTypeBuilders chargeTypeBuilders)
        {
            Charge = chargeTypeBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<IChargeTypeBuilder>
            {
                { "endurance charges?", Charge.Endurance },
                { "frenzy charges?", Charge.Frenzy },
                { "power charges?", Charge.Power },
                { "rage", Charge.Rage },
            };
    }
}