using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;

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
        }.ToList();
    }
}