using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IEffectStats"/> implementation. Provides the stats applied when effects and flags are active.
    /// </summary>
    public class EffectStats : UsesStatBuilders, IEffectStats
    {
        private readonly Lazy<IReadOnlyList<EffectStatData>> _lazyEffects;

        public EffectStats(IBuilderFactories builderFactories)
            : base(builderFactories)
        {
            _lazyEffects = new Lazy<IReadOnlyList<EffectStatData>>(() => CreateEffectCollection().ToList());
        }

        public IReadOnlyList<EffectStatData> Effects => _lazyEffects.Value;

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
            { Buff.Onslaught, "20% increased attack, cast and movement speed" },
            { Buff.UnholyMight, "gain 30% of physical damage as extra chaos damage" },
            { Buff.Conflux.Igniting, Ailment.Ignite.Sources(AllDamageTypes) },
            { Buff.Conflux.Shocking, Ailment.Shock.Sources(AllDamageTypes) },
            { Buff.Conflux.Chilling, Ailment.Chill.Sources(AllDamageTypes) },
            {
                Buff.Conflux.Elemental,
                Ailment.Ignite.Sources(AllDamageTypes), Ailment.Shock.Sources(AllDamageTypes),
                Ailment.Chill.Sources(AllDamageTypes)
            },
        };
    }
}