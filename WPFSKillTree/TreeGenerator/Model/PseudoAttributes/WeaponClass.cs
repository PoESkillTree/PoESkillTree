using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Enumeration of weapon classes a character can have.
    /// </summary>
    public enum WeaponClass
    {
        [Description("Bow")]
        Bow,
        [Description("Wand")]
        Wand,
        [Description("Claw")]
        Claw,
        [Description("Dagger")]
        Dagger,
        [Description("One Hand Axe")]
        OneHandAxe,
        [Description("One Hand Mace")]
        OneHandMace,
        [Description("One Hand Sword")]
        OneHandSword,
        [Description("Sceptre")]
        Sceptre,
        [Description("Staff")]
        Staff,
        [Description("Two Hand Axe")]
        TwoHandAxe,
        [Description("Two Hand Mace")]
        TwoHandMace,
        [Description("Two Hand Sword")]
        TwoHandSword,
        [Description("Unarmed")]
        Unarmed
    }

    public static class WeaponClassExtensions
    {
        private static readonly Dictionary<WeaponClass, string[]> Aliases = new Dictionary<WeaponClass, string[]>()
        {
            {WeaponClass.Bow, new [] {"bow", "bows"} },
            {WeaponClass.Wand, new [] {"wand", "wands"} },

            {WeaponClass.Claw,  new [] {"claw", "claws", "one handed melee weapons"}},
            {WeaponClass.Dagger, new [] {"dagger", "daggers", "one handed melee weapons"} },
            {WeaponClass.OneHandAxe, new [] {"axe", "axes", "one handed melee weapons"} },
            {WeaponClass.OneHandMace, new [] {"mace", "maces", "one handed melee weapons"} },
            {WeaponClass.OneHandSword, new [] {"sword", "swords", "one handed melee weapons"} },
            {WeaponClass.Sceptre, new [] {"sceptre", "sceptres", "one handed melee weapons" } },

            {WeaponClass.Staff, new [] {"staff", "staves", "two handed melee weapons"} },
            {WeaponClass.TwoHandAxe, new [] {"axe", "axes", "two handed melee weapons" } },
            {WeaponClass.TwoHandMace, new [] {"mace", "maces", "two handed melee weapons" } },
            {WeaponClass.TwoHandSword, new [] {"sword", "swords", "two handed melee weapons" } },

            {WeaponClass.Unarmed, new [] {"unarmed"} }
        };

        private static readonly HashSet<WeaponClass> TwoHandedClasses = new HashSet<WeaponClass>()
        {
            WeaponClass.Staff,
            WeaponClass.TwoHandAxe,
            WeaponClass.TwoHandMace,
            WeaponClass.TwoHandSword,
            WeaponClass.Bow
        };

        /// <summary>
        /// Returns whether the given WeaponClass has the given string as an alias.
        /// (case insensitive)
        /// </summary>
        public static bool HasAlias(this WeaponClass weaponClass, string alias)
        {
            if (alias == null) return false;
            alias = alias.ToLowerInvariant();
            return Aliases[weaponClass].Any(s => s == alias);
        }

        /// <summary>
        /// Returns whether the given WeaponClass describes a two handed weapon.
        /// </summary>
        /// <param name="weaponClass"></param>
        /// <returns></returns>
        public static bool IsTwoHanded(this WeaponClass weaponClass)
        {
            return TwoHandedClasses.Contains(weaponClass);
        }
    }
}