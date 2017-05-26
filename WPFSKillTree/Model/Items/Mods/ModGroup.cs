using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// A group of mods with the same <see cref="JsonMod.Group"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(Group) + "}")]
    public class ModGroup
    {
        public string Group { get; }

        private readonly IReadOnlyList<Mod> _mods;

        public IReadOnlyList<IMod> Mods => _mods;

        public ModGroup(string group, IEnumerable<Mod> mods)
        {
            Group = group;
            _mods = mods.ToList();
        }

        /// <returns>all mods in this group that can be can be crafted onto an item with the given tags and class
        /// </returns>
        public IEnumerable<IMod> GetMatchingMods(Tags tags, ItemClass itemClass)
        {
            return _mods.Where(m => m.Matches(tags, itemClass));
        }
    }
}