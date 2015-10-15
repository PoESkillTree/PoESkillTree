using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Tags that specify how damage is dealt/what improves the damage dealt.
    /// </summary>
    [Flags]
    public enum Tags
    {
        None = 0, Attack = 1, Melee = 2, Duration = 4, Projectile = 8,
        Spell = 16, Trap = 32, Mine = 64, Totem = 128, Area = 256, Cast = 512,
        // Curse is unused. Curse nodes are so sparse that there won't be many choices.
        // Also since Tags are set global one would need to specialize only for curses.
        Curse = 1024
    }

    public static class TagsExtensions
    {
        private static readonly Dictionary<Tags, string[]> Aliases = new Dictionary<Tags, string[]>()
        {
            {Tags.Attack, new [] {"attack", "attacks"} },
            {Tags.Melee, new [] {"melee"} },
            {Tags.Duration, new [] {"dot", "damage over time", "duration"} },
            {Tags.Projectile, new [] {"projectile", "projectiles"} },
            {Tags.Spell, new [] {"spell", "spells"} },
            {Tags.Trap, new [] {"trap", "traps"} },
            {Tags.Mine, new [] {"mine", "mines"} },
            {Tags.Totem, new [] {"totem", "totems", "totem skills"} },
            {Tags.Area, new [] {"area", "aoe", "area of effect"} },
            {Tags.Cast, new [] {"cast", "casts"} },
            {Tags.Curse, new [] {"curse", "curses"} }
        };

        /// <summary>
        /// Returns whether any of the given Tags has the given string as an alias.
        /// (case insensitive)
        /// </summary>
        public static bool HasAlias(this Tags tags, string alias)
        {
            if (alias == null || tags == Tags.None) return false;
            alias = alias.ToLowerInvariant();
            return Aliases.Any(pair => tags.HasFlag(pair.Key) && pair.Value.Any(s => s == alias));
        }
    }
}