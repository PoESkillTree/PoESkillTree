using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public enum WeaponClass
    {
        Bow, Wand,
        Claw, Dagger, OneHandedAxe, OneHandedMace, OneHandedSword, Sceptre,
        Staff, TwoHandedAxe, TwoHandedMace, TwoHandedSword,
        Unarmed
    }

    public static class WeapnClassExtensions
    {
        public static readonly Dictionary<WeaponClass, string[]> Aliases = new Dictionary<WeaponClass, string[]>()
        {
            {WeaponClass.Bow, new [] {"bow", "bows"} },
            {WeaponClass.Wand, new [] {"wand", "wands"} },

            {WeaponClass.Claw,  new [] {"claw", "claws", "one handed melee weapons"}},
            {WeaponClass.Dagger, new [] {"dagger", "daggers", "one handed melee weapons"} },
            {WeaponClass.OneHandedAxe, new [] {"axe", "axes", "one handed melee weapons"} },
            {WeaponClass.OneHandedMace, new [] {"mace", "maces", "one handed melee weapons"} },
            {WeaponClass.OneHandedSword, new [] {"sword", "swords", "one handed melee weapons"} },
            {WeaponClass.Sceptre, new [] {"sceptre", "sceptres", "one handed melee weapons" } },

            {WeaponClass.Staff, new [] {"staff", "staves", "two handed melee weapons"} },
            {WeaponClass.TwoHandedAxe, new [] {"axe", "axes", "two handed melee weapons" } },
            {WeaponClass.TwoHandedMace, new [] {"mace", "maces", "two handed melee weapons" } },
            {WeaponClass.TwoHandedSword, new [] {"sword", "swords", "two handed melee weapons" } },

            {WeaponClass.Unarmed, new [] {"unarmed"} }
        };

        public static bool HasAlias(this WeaponClass weaponClass, string alias)
        {
            alias = alias.ToLowerInvariant();
            return Aliases.Any(pair => pair.Key == weaponClass && pair.Value.Any(s => s == alias));
        }
    }
}