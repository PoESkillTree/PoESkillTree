using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model
{
    [Flags]
    public enum Tags
    {
        None = 0, Attack = 1, Melee = 2, DoT = 4
    }

    public static class TagsExtensions
    {
        private static readonly Dictionary<Tags, string[]> Aliases = new Dictionary<Tags, string[]>()
        {
            {Tags.Attack, new [] {"attack", "attacks"} },
            {Tags.Melee, new [] {"melee"} },
            {Tags.DoT, new [] {"dot", "damage over time"} }
        };

        public static bool HasAlias(this Tags tags, string alias)
        {
            alias = alias.ToLowerInvariant();
            return Aliases.Any(pair => tags.HasFlag(pair.Key) && pair.Value.Any(s => s == alias));
        }
    }
}