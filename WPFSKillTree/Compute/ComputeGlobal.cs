using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.SkillTreeFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree.Compute
{
    public static class ComputeGlobal
    {
        // Monster average accuracy for each level (1 .. 100).
        public readonly static int[] MonsterAverageAccuracy = new int[] { 0, // Level 0 placeholder.
              18,     19,     20,     21,     23,
              24,     25,     27,     28,     30,
              31,     33,     35,     36,     38,
              40,     42,     44,     46,     49,
              51,     54,     56,     59,     62,
              65,     68,     71,     74,     78,
              81,     85,     89,     93,     97,
             101,    106,    111,    116,    121,
             126,    132,    137,    143,    149,
             156,    162,    169,    177,    184,
             192,    200,    208,    217,    226,
             236,    245,    255,    266,    277,
             288,    300,    312,    325,    338,
             352,    366,    381,    396,    412,
             428,    445,    463,    481,    500,
             520,    540,    562,    584,    607,
             630,    655,    680,    707,    734,
             762,    792,    822,    854,    887,
             921,    956,    992,   1030,   1069,
            1110,   1152,   1196,   1241,   1288
        };
        // Monster average evasion rating for each level (1 .. 100).
        public readonly static int[] MonsterAverageEvasion = new int[] { 0, // Level 0 placeholder.
              36,     42,     49,     56,     64,
              72,     80,     89,     98,    108,
             118,    128,    140,    151,    164,
             177,    190,    204,    219,    235,
             251,    268,    286,    305,    325,
             345,    367,    389,    412,    437,
             463,    489,    517,    546,    577,
             609,    642,    676,    713,    750,
             790,    831,    873,    918,    964,
            1013,   1063,   1116,   1170,   1227,
            1287,   1349,   1413,   1480,   1550,
            1623,   1698,   1777,   1859,   1944,
            2033,   2125,   2221,   2321,   2425,
            2533,   2645,   2761,   2883,   3009,
            3140,   3276,   3418,   3565,   3717,
            3876,   4041,   4213,   4391,   4576,
            4768,   4967,   5174,   5389,   5613,
            5845,   6085,   6335,   6595,   6864,
            7144,   7434,   7735,   8048,   8372,
            8709,   9058,   9420,   9796,  10186
        };

        // The sorted list of damage types for character sheet.
        public static List<DamageType> DamageTypes = new List<DamageType>()
        {
            DamageType.Total, DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos
        };
        // The dictionary of weapon hands.
        public static Dictionary<string, WeaponHand> WithWeaponHand = new Dictionary<string, WeaponHand>()
        {
            { "Main Hand",                  WeaponHand.Main },
            { "Off Hand",                   WeaponHand.Off }
        };
        // The dictionary of weapon types.
        public static Dictionary<string, WeaponType> WithWeaponType = new Dictionary<string, WeaponType>()
        {
            { "Bows",                       WeaponType.Bow },
            { "Claws",                      WeaponType.Claw },
            { "Daggers",                    WeaponType.Dagger },
            { "Wands",                      WeaponType.Wand },
            { "One Handed Melee Weapons",   WeaponType.OneHandedMelee },
            { "Two Handed Melee Weapons",   WeaponType.TwoHandedMelee },
            { "Axes",                       WeaponType.Axe },
            { "Maces",                      WeaponType.Mace },
            { "Staves",                     WeaponType.Staff },
            { "Swords",                     WeaponType.Sword }
        };

        // Returns value increased by specified percentage.
        public static float IncreaseValueByPercentage(float value, float percentage)
        {
            return value * (100 + percentage) / 100;
        }

        // Returns percent of value.
        public static float PercentOfValue(float value, float percentage)
        {
            return value * percentage / 100;
        }

        // Damage Reduction Factor = Armour / ( Armour + (10 * Damage) )
        // Damage reduction is capped at 90%.
        // @see http://pathofexile.gamepedia.com/Armour
        public static float PhysicalDamageReduction(int level, float armour)
        {
            float mad = MonsterAverageDamage(Math.Min(level, 80) - 1);
            float reduction = RoundValue(armour / (armour + 10 * mad) * 100, 1);
            if (reduction > 90f) reduction = 90f;

            return reduction;
        }

        // Returns rounded value with specified number of fractional digits (round half down if even digit before half).
        public static float RoundHalfDownEvenValue(float value, int precision)
        {
            // Detect half.
            float coeff = (float)Math.Pow(10, precision);
            float half = value * coeff;

            return (half - (int)half == 0.5 || half - (int)half == -0.5) && (int)half % 2 == 0
                   ? (float)((int)half) / coeff
                   : (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }

        // Returns rounded value with specified number of fractional digits (round half down).
        public static float RoundHalfDownValue(float value, int precision)
        {
            // Detect half.
            float coeff = (float)Math.Pow(10, precision);
            float half = value * coeff;

            return half - (int)half == 0.5 || half - (int)half == -0.5
                   ? (float)((int)half) / coeff
                   : (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }

        // Returns rounded value with specified number of fractional digits.
        public static float RoundValue(float value, int precision)
        {
            return (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }

        // Returns value capped at specified maximum.
        public static float MaximumValue(float value, float maximum)
        {
            return value <= maximum ? value : maximum;
        }

        // Returns average damage done by monsters at specified character level.
        // @see http://pathofexile.gamepedia.com/Monster_Damage
        public static float MonsterAverageDamage(int level)
        {
            return RoundValue((float)(0.0015 * Math.Pow(level, 3) + 0.2 * level + 6), 0);
        }
        // Returns rounded value with all fractional digits after specified precision cut off.
        public static float FloorValue(float value, int precision)
        {
            float coeff = (float)Math.Pow(10, precision);

            return (float)(Math.Floor((float)(value * coeff)) / coeff);
        }

        // Returns rounded value with all fractional digits after specified precision cut off.
        public static float CeilValue(float value, int precision)
        {
            float coeff = (float)Math.Pow(10, precision);

            return (float)(Math.Ceiling((float)(value * coeff)) / coeff);
        }
    }
}
