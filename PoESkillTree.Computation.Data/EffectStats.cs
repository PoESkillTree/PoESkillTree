using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;

namespace PoESkillTree.Computation.Data
{
    public class EffectStats : UsesStatProviders, IEffectStats
    {
        public EffectStats(IProviderFactories providerFactories)
            : base(providerFactories)
        {
            Effects = CreateEffectCollection().ToList();
            Flags = CreateFlagCollection().ToList();
        }

        public IReadOnlyList<EffectStatData> Effects { get; }
        public IReadOnlyList<FlagStatData> Flags { get; }

        private EffectStatCollection CreateEffectCollection() => new EffectStatCollection
        {
            // ailments
            { Ailment.Shock, "50% increased damage taken" },
            { Ailment.Chill, "30% reduced animation speed" },
            { Ailment.Freeze, "100% reduced animation speed" },
            // buffs
            { Buff.Fortify, "20% reduced damage taken from hits" },
            { Buff.Maim, "30% reduced movement speed" },
            { Buff.Intimidate, "10% increased damage taken" },
            { Buff.Blind, "50% less chance to hit" },
            { Buff.Conflux.Igniting, Ailment.Ignite.AddSources(AllDamageTypes) },
            { Buff.Conflux.Shocking, Ailment.Shock.AddSources(AllDamageTypes) },
            { Buff.Conflux.Chilling, Ailment.Chill.AddSources(AllDamageTypes) },
            {
                Buff.Conflux.Elemental,
                Ailment.Ignite.AddSources(AllDamageTypes), Ailment.Shock.AddSources(AllDamageTypes),
                Ailment.Chill.AddSources(AllDamageTypes)
            },
        };

        private FlagStatCollection CreateFlagCollection() => new FlagStatCollection
        {
            { Flag.Onslaught, "20% increased attack, cast and movement speed" },
            { Flag.UnholyMight, "gain 30% of physical damage as extra chaos damage" },
        };
    }
}