using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;

namespace PoESkillTree.Computation.Data
{
    // see http://pathofexile.gamepedia.com/Character and Metadata/Characters/Character.ot in GGPK
    public class CharacterGivenStats : UsesStatProviders, IGivenStats
    {
        public CharacterGivenStats(IProviderFactories providerFactories)
            : base(providerFactories)
        {
            GivenStats = CreateCollection();
        }

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "15% additional Block Chance while Dual Wielding",
            "20% more Attack Physical Damage while Dual Wielding",
            // charges
            "4% additional Physical Damage Reduction per Endurance Charge",
            "+4% to all Elemental Resistances per Endurance Charge",
            "4% increased Attack Speed per Frenzy Charge",
            "4% increased Cast Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "50% increased Critical Strike Chance per Power Charge",
            // Rampage
            "1% increased Movement Speed per 10 Rampage Stacks",
            "2% increased Damage per 10 Rampage Stacks",
            "Minions deal 2% increased Damage per 10 Rampage Stacks",
            "Minions gain 1% increased Movement Speed per 10 Rampage Stacks",
            // level based
            "+12 to maximum Life per Level",
            "+2 to Accuracy Rating per Level",
            "+3 to Evasion Rating per Level",
            "+6 to maximum Mana per Level",
            // attribute conversions
            "+1 to maximum Life per 2 Strength",
            "+1 to Strength Damage Bonus per Strength",
            "1% increased Melee Physical Damage per 5 Strength Damage Bonus ceiled",
            "+2 to Accuracy Rating per 1 Dexterity",
            "+1 to Dexterity Evasion Bonus per Dexterity",
            "1% increased Evasion Rating per 5 Dexterity Evasion Bonus ceiled",
            "+1 Mana per 2 Intelligence ceiled",
            "1% increased maximum Energy Shield per 5 Intelligence ceiled",
            // ailments
            "100% chance to ignite on critical strike",
            "100% chance to shock on critical strike",
            "100% chance to chill",
            "100% chance to freeze on critical strike",
            // other
            "100% of non-chaos damage is taken from energy shield before life"
        };

        public IEnumerable<object> GivenStats { get; }

        private GivenStatsCollection CreateCollection() => new GivenStatsCollection
        {
            // base stats
            { BaseSet, Life, 38 },
            { BaseSet, Mana, 34 },
            { BaseSet, Evasion, 53 },
            { BaseSet, Stat.Accuracy, -2 }, // 0 at level 1 with no dexterity
            { BaseSet, Mana.Regen.Percent, 1.75 },
            { BaseSet, EnergyShield.Recharge, 20 },
            { BaseSet, CriticalStrike.Multiplier, 150 },
            { BaseSet, CriticalStrike.AilmentMultiplier, 150 },
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
            { BaseSet, Physical.Resistance.Maximum, 90 },
            // - traps, mines and totems
            { BaseSet, Traps.CombinedInstances.Maximum, 3 },
            { BaseSet, Mines.CombinedInstances.Maximum, 5 },
            { BaseSet, Totems.CombinedInstances.Maximum, 1 },
            // - buffs
            { BaseSet, Buffs(target: Self).With(Keyword.Curse).CombinedLimit, 1 },
        };
    }
}