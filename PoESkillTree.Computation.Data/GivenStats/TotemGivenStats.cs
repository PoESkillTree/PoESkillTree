using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

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
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public TotemGivenStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Totem };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // Totems get 5% per charge, other monsters 15%. 15 - 10 = 5.
            "-10% to Physical Damage Reduction per Endurance Charge",
            "-10% to all Elemental Resistances per Endurance Charge",

            "45% less damage taken",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            { TotalOverride, Charge.Frenzy.Amount.Maximum, 0 },
            { TotalOverride, Charge.Power.Amount.Maximum, 0 },
            { TotalOverride, Buff.CurseLimit, 0 },
            { TotalOverride, Mana.Cost, 0 },
            { BaseAdd, Elemental.Resistance.Maximum, 40 },
            { BaseAdd, Chaos.Resistance.Maximum, 20 },
        };
    }
}