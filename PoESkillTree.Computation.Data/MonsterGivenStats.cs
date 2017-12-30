using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// Given stats of all monsters.
    /// </summary>
    /// <remarks>
    /// See https://pathofexile.gamepedia.com/Monster and Metadata/Monsters/Monster.ot in GGPK.
    /// </remarks>
    public class MonsterGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly Lazy<IReadOnlyList<GivenStatData>> _lazyGivenStats;

        public MonsterGivenStats(IBuilderFactories builderFactories) : base(builderFactories)
        {
            _lazyGivenStats = new Lazy<IReadOnlyList<GivenStatData>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            "15% additional Physical Damage Reduction per Endurance Charge",
            "+15% to all Elemental Resistances per Endurance Charge",
            "15% increased Attack Speed per Frenzy Charge",
            "15% increased Cast Speed per Frenzy Charge",
            "5% increased Movement Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "200% increased Critical Strike Chance per Power Charge",
        };

        public IReadOnlyList<GivenStatData> GivenStats => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection
        {
            // base stats
            { BaseSet, Mana, 200 },
            { BaseSet, Mana.Regen.Percent, 100 / 60.0 },
            { BaseSet, EnergyShield.Recharge, 20 },
            { BaseSet, CriticalStrike.Multiplier, 130 },
            { BaseSet, CriticalStrike.AilmentMultiplier, 130 },
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
            // - leech
            { BaseSet, Life.Leech.RateLimit, 20 },
            { BaseSet, Mana.Leech.RateLimit, 20 },
            // - resistances
            { BaseSet, Elemental.Resistance.Maximum, 75 },
            { BaseSet, Chaos.Resistance.Maximum, 75 },
            { BaseSet, Physical.Resistance.Maximum, 75 },
            // - traps, mines and totems
            { BaseSet, Traps.CombinedInstances.Maximum, 3 },
            { BaseSet, Mines.CombinedInstances.Maximum, 5 },
            { BaseSet, Totems.CombinedInstances.Maximum, 1 },
            // - buffs
            { BaseSet, Buffs(target: Self).With(Keyword.Curse).CombinedLimit, 1 },
            // - movement speed
            { BaseSet, Stat.MovementSpeed.Maximum, 128 },
        };
    }
}