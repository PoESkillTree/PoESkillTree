using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Damage;

namespace PoESkillTree.Computation.Data
{
    public class DamageTypeMatchers : ReferencedMatchersBase<IDamageTypeProvider>
    {
        private readonly IDamageTypeProviderFactory _damageTypeProviderFactory;

        public DamageTypeMatchers(IDamageTypeProviderFactory damageTypeProviderFactory)
        {
            _damageTypeProviderFactory = damageTypeProviderFactory;
        }

        private IDamageTypeProvider Physical => _damageTypeProviderFactory.Physical;
        private IDamageTypeProvider Fire => _damageTypeProviderFactory.Fire;
        private IDamageTypeProvider Lightning => _damageTypeProviderFactory.Lightning;
        private IDamageTypeProvider Cold => _damageTypeProviderFactory.Cold;
        private IDamageTypeProvider Chaos => _damageTypeProviderFactory.Chaos;

        protected override IEnumerable<ReferencedMatcherData<IDamageTypeProvider>>
            CreateCollection() =>
            new ReferencedMatcherCollection<IDamageTypeProvider>
            {
                { "physical", Physical },
                { "fire", Fire },
                { "lightning", Lightning },
                { "cold", Cold },
                { "chaos", Chaos },
                // combinations
                { "elemental", Fire.And(Lightning).And(Cold) },
                { "physical, cold and lightning", Physical.And(Cold).And(Lightning) },
                { "physical and fire", Physical.And(Fire) },
                // inverse
                { "non-fire", Fire.Invert },
                { "non-chaos", Chaos.Invert },
            };
    }
}