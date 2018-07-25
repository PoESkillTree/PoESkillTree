using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Totem-specific given stats.
    /// </summary>
    /// <remarks>
    /// See Metadata/Monsters/Totems/TotemBase.ot and Totem.ot in GGPK.
    /// </remarks>
    public class TotemGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly Lazy<IReadOnlyList<GivenStatData>> _lazyGivenStats;

        public TotemGivenStats(IBuilderFactories builderFactories) : base(builderFactories)
        {
            _lazyGivenStats = new Lazy<IReadOnlyList<GivenStatData>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { Common.Entity.Totem };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // Totems get 5% per charge, other monsters 15%. 15 - 10 = 5.
            "-10% to Physical Damage Reduction per Endurance Charge",
            "-10% to all Elemental Resistances per Endurance Charge",

            "45% less damage taken",
            "+40% to all Elemental Resistances",
            "+20% to Chaos Resistance",
        };

        public IReadOnlyList<GivenStatData> GivenStats => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection
        {
            { TotalOverride, Charge.Frenzy.Amount.Maximum, 0 },
            { TotalOverride, Charge.Power.Amount.Maximum, 0 },
            { TotalOverride, Buff.CurseLimit, 0 },
            { TotalOverride, Mana.Cost, 0 },
        };
    }
}