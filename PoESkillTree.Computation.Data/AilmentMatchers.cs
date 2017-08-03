using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Effects;

namespace PoESkillTree.Computation.Data
{
    public class AilmentMatchers : IReferencedMatchers<IAilmentProvider>
    {
        private IAilmentProviderFactory Ailment { get; }

        public AilmentMatchers(IAilmentProviderFactory ailmentProviderFactory)
        {
            Ailment = ailmentProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        public string ReferenceName { get; } = nameof(AilmentMatchers);

        public IReadOnlyList<ReferencedMatcherData<IAilmentProvider>> Matchers { get; }

        private MatcherCollection<IAilmentProvider> CreateCollection() =>
            new MatcherCollection<IAilmentProvider>
            {
                // chance to x/x duration
                { "ignite", Ailment.Ignite },
                { "shock", Ailment.Shock },
                { "chill", Ailment.Chill },
                { "freeze", Ailment.Freeze },
                { "bleed", Ailment.Bleed },
                { "cause bleeding", Ailment.Bleed },
                { "poison", Ailment.Poison },
                // being/while/against x
                { "ignited", Ailment.Ignite },
                { "shocked", Ailment.Shock },
                { "chilled", Ailment.Chill },
                { "frozen", Ailment.Freeze },
                { "bleeding", Ailment.Bleed },
                { "poisoned", Ailment.Poison },
            };
    }
}