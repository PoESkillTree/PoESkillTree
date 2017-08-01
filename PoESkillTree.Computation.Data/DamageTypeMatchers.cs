using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Damage;

namespace PoESkillTree.Computation.Data
{
    public class DamageTypeMatchers : IReferencedMatchers<IDamageTypeProvider>
    {
        private readonly IDamageTypeProviderFactory _damageTypeProviderFactory;

        public DamageTypeMatchers(IDamageTypeProviderFactory damageTypeProviderFactory)
        {
            _damageTypeProviderFactory = damageTypeProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        private IDamageTypeProvider Physical => _damageTypeProviderFactory.Physical;
        private IDamageTypeProvider Fire => _damageTypeProviderFactory.Fire;
        private IDamageTypeProvider Lightning => _damageTypeProviderFactory.Lightning;
        private IDamageTypeProvider Cold => _damageTypeProviderFactory.Cold;
        private IDamageTypeProvider Chaos => _damageTypeProviderFactory.Chaos;

        public IReadOnlyList<(string regex, IDamageTypeProvider match)> Matchers { get; }

        private MatcherCollection<IDamageTypeProvider> CreateCollection() => 
            new MatcherCollection<IDamageTypeProvider>
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