using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats shared between all entities.
    /// </summary>
    public class CommonGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly Lazy<IReadOnlyList<GivenStatData>> _lazyGivenStats;

        public CommonGivenStats(IBuilderFactories builderFactories) : base(builderFactories)
        {
            _lazyGivenStats = new Lazy<IReadOnlyList<GivenStatData>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // Rampage
            "1% increased Movement Speed per 10 Rampage Stacks",
            "2% increased Damage per 10 Rampage Stacks",
            "Minions deal 2% increased Damage per 10 Rampage Stacks",
            "Minions gain 1% increased Movement Speed per 10 Rampage Stacks",
            // ailments
            "100% chance to ignite on critical strike",
            "100% chance to shock on critical strike",
            "100% chance to chill",
            "100% chance to freeze on critical strike",
            // other
            "100% of non-chaos damage is taken from energy shield before life",
        };

        public IReadOnlyList<GivenStatData> GivenStats => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection
        {
            // base stats
            { BaseSet, EnergyShield.Recharge, 20 },
            { BaseSet, CriticalStrike.Multiplier, 130 },
            { BaseSet, Buff.CurseLimit, 1 },
            // minima and maxima
            // - crit
            { BaseSet, CriticalStrike.Chance.Maximum, 95 },
            { BaseSet, CriticalStrike.Chance.Minimum, 5 },
            // - evasion
            { BaseSet, Evasion.Chance.Maximum, 95 },
            { BaseSet, Evasion.Chance.Minimum, 5 },
            // - block
            { BaseSet, Block.AttackChance.Maximum, 75 },
            { BaseSet, Block.SpellChance.Maximum, 75 },
            // - dodge
            { BaseSet, Stat.Dodge.AttackChance.Maximum, 75 },
            { BaseSet, Stat.Dodge.SpellChance.Maximum, 75 },
            // - charges
            { BaseSet, Charge.Endurance.Amount.Maximum, 3 },
            { BaseSet, Charge.Frenzy.Amount.Maximum, 3 },
            { BaseSet, Charge.Power.Amount.Maximum, 3 },
            // - Rampage
            { BaseSet, Stat.RampageStacks.Maximum, 1000 },
            // - leech
            { BaseSet, Life.Leech.RateLimit, 20 },
            { BaseSet, Mana.Leech.RateLimit, 20 },
            // - resistances
            { BaseSet, Elemental.Resistance.Maximum, 75 },
            { BaseSet, Chaos.Resistance.Maximum, 75 },
        };
    }
}