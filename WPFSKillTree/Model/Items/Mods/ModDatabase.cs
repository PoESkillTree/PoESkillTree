using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    public class ModDatabase
    {
        private readonly IReadOnlyDictionary<string, Mod> _mods;
        private readonly IReadOnlyDictionary<ModType, IEnumerable<Affix>> _affixesByType;

        public IEnumerable<Affix> this[ModType modtype] => _affixesByType[modtype];

        public ModDatabase(IReadOnlyDictionary<string, JsonMod> mods, IEnumerable<JsonCraftingBenchOption> masterMods)
        {
            var masterLookup = masterMods.ToLookup(m => m.ModId);
            _mods = mods.ToDictionary(p => p.Key, p => new Mod(p.Key, p.Value, masterLookup[p.Key]));
            _affixesByType = _mods.Values
                .GroupBy(m => m.JsonMod.GenerationType)
                .ToDictionary(g => g.Key, ModsToAffixes);
        }

        private static IEnumerable<Affix> ModsToAffixes(IEnumerable<Mod> mods)
        {
            return mods
                .GroupBy(m => m.JsonMod.Group)
                .Select(g => new Affix(g.Key, g));
        }
    }
}