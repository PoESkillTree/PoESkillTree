using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.GameModel.Modifiers;

namespace PoESkillTree.Model.Items.Mods
{
    /// <summary>
    /// Provides access to mods.
    /// </summary>
    public class ModDatabase
    {
        /// <summary>
        /// Gets a dictionary with mod ids as keys and the mods as values
        /// </summary>
        public IReadOnlyDictionary<string, Mod> Mods { get; }

        private readonly IReadOnlyDictionary<ModGenerationType, IReadOnlyList<ModGroup>> _groupsByType;

        /// <returns>the mod groups in this database with the given generation type</returns>
        public IReadOnlyList<ModGroup> this[ModGenerationType modtype] => _groupsByType[modtype];

        public ModDatabase(IReadOnlyDictionary<string, JsonMod> mods, IEnumerable<JsonCraftingBenchOption> benchOptions)
        {
            var benchLookup = benchOptions
                .Where(x => x.Actions.AddMod != null)
                .ToLookup(x => x.Actions.AddMod!);
            Mods = mods.ToDictionary(
                p => p.Key, 
                p => new Mod(p.Key, p.Value, benchLookup[p.Key]));
            _groupsByType = Mods.Values
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