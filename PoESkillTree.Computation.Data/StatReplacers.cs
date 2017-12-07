using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    public class StatReplacers
    {
        // These simply replace a stat line that would get thrown into the matchers with another
        // Reduces redundant handling in some cases and allows elegantly solving other cases
        // As opposed to the matchers, values are not replaced by # placeholders here

        public IReadOnlyList<StatReplacerData> Replacers { get; } = new StatReplacerCollection
        {
            {
                // Grand Spectrum
                @"(.+) per grand spectrum",
                "grand spectrum", "$0"
            },
            {
                // Corrupted Energy Cobalt Jewel
                @"(with \d corrupted items Equipped:) (\d+% of chaos damage does not bypass energy shield), and (\d+% of physical damage bypasses energy shield)",
                "$1 $2", "$1 $3"
            },
            // keystones
            // (some need to be manually split, others are renamed to need no further custom handling)
            {
                // Acrobatics
                @"(\d+% chance to dodge attacks)\. (\d+% less armour and energy shield), (\d+% less chance to block spells and attacks)",
                "$1", "$2", "$3"
            },
            {
                // Eldritch Battery, second stat
                "energy shield protects mana instead of life",
                "100% of non-chaos damage is taken from energy shield before mana",
                "-100% of non-chaos damage is taken from energy shield before life"
            },
            {
                // Chaos Inoculation
                "(maximum life becomes 1), (immune to chaos damage)",
                "$1", "$2"
            },
            {
                // Blood Magic
                @"(removes all mana)\. (spend .*)",
                "$1", "$2"
            },
            {
                // Iron Reflexes
                @"(converts all evasion rating to armour)\. (dexterity provides no bonus to evasion rating)",
                "$1", "-1 to dexterity evasion bonus per dexterity"
            },
            {
                // Iron Grip
                "the increase to physical damage from strength applies to projectile attacks as well as melee attacks",
                "1% increased physical projectile attack damage per 5 strength damage bonus ceiled"
            },
            {
                // Vaal Pact
                @"life leeched per second is doubled\. (maximum life leech rate is doubled)\. (life regeneration has no effect)\.",
                "100% increased life leeched per second", "$1", "$2"
            },
            {
                // Ancestral Bond
                "(you can't deal damage with skills yourself) (can have up to 1 additional totem summoned at a time)",
                "$1", "$2"
            },
            {
                // Ghost Reaver
                @"(life leech is applied to energy shield instead) (\d+% less energy shield recharge rate)",
                "$1", "$2"
            },
            {
                // Arrow Dancing
                @"(\d+% more chance to evade projectile attacks) (\d+% less chance to evade melee attacks)",
                "$1", "$2"
            },
            {
                // Elemental Overload
                @"(\d+% more elemental damage if you've crit in the past \d+ seconds) (no critical strike multiplier) (no damage multiplier for ailments from critical strikes)",
                "$1", "$2", "$3"
            },
            {
                // Avatar of Fire
                @"(\d+% of physical, cold and lightning damage converted to fire damage) (deal no non-fire damage)",
                "$1", "$2"
            },
            {
                // Unwavering Stance
                @"(cannot evade enemy attacks) (cannot be stunned)",
                "$1", "$2"
            },
            {
                // Perfect Agony
                @"(modifiers to critical strike multiplier also apply to damage multiplier for ailments from critical strikes at \d+% of their value) (\d+% less damage with hits)",
                "$1", "$2"
            },
            {
                // Crimson Dance
                @"(you can inflict bleeding on an enemy up to \d+ times) (your bleeding does not deal extra damage while the enemy is moving)",
                "$1", "$2"
            },
            {
                // Resolute Technique
                @"(your hits can't be evaded) (never deal critical strikes)",
                "$1", "$2"
            },
        }.ToList();
    }
}