using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Mods
{
    public class ModDatabase
    {
        private readonly IReadOnlyDictionary<string, Mod> _mods;
        private readonly IReadOnlyDictionary<ModGenerationType, IReadOnlyList<ModGroup>> _groupsByType;

        public IMod this[string modId] => _mods[modId];
        public IReadOnlyList<ModGroup> this[ModGenerationType modtype] => _groupsByType[modtype];

        public ModDatabase(IReadOnlyDictionary<string, JsonMod> mods, IEnumerable<JsonCraftingBenchOption> benchOptions,
            IReadOnlyDictionary<string, JsonNpcMaster> npcMasters)
        {
            var benchLookup = benchOptions.ToLookup(m => m.ModId);
            var signatureModDict = npcMasters
                .Select(n => n.Value.SignatureMod)
                .ToDictionary(s => s.Id, s => s.SpawnTags);
            _mods = mods.ToDictionary(
                p => p.Key, 
                p => new Mod(p.Key, p.Value, benchLookup[p.Key], signatureModDict.GetOrDefault(p.Key)));
            _groupsByType = _mods.Values
                .GroupBy(m => m.JsonMod.GenerationType)
                .ToDictionary(g => g.Key, ModsToAffixes);
        }

        private static IReadOnlyList<ModGroup> ModsToAffixes(IEnumerable<Mod> mods)
        {
            return mods
                .GroupBy(m => m.JsonMod.Group)
                .Select(g => new ModGroup(g.Key, g))
                .ToList();
        }
    }
}