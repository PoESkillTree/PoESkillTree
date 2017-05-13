using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    [DebuggerDisplay("{" + nameof(Group) + "}")]
    public class Affix
    {
        public string Group { get; }

        private readonly IReadOnlyList<Mod> _mods;

        public IReadOnlyList<IMod> Mods => _mods;

        public Affix(string group, IEnumerable<Mod> mods)
        {
            Group = group;
            _mods = mods.ToList();
        }

        public IEnumerable<IMod> GetMatchingMods(ModDomain domain, Tags tags, ItemClass itemClass)
        {
            return _mods.Where(m => m.Matches(domain, tags, itemClass));
        }
    }
}