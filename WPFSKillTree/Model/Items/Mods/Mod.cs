using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Mod : IMod
    {
        public string Id { get; }

        private readonly ISet<ItemClass> _itemClasses = new HashSet<ItemClass>();
        private readonly IList<Tuple<Tags, bool>> _spawnTags = new List<Tuple<Tags, bool>>();

        public JsonMod JsonMod { get; }

        public IReadOnlyList<Stat> Stats { get; }
        public ModDomain Domain => JsonMod.Domain;
        public bool IsEssenceOnly => JsonMod.IsEssenceOnly;

        public Mod(string id, JsonMod jsonMod, IEnumerable<JsonCraftingBenchOption> jsonMasterMods)
        {
            Id = id;
            foreach (var jsonMasterMod in jsonMasterMods)
            {
                foreach (var itemClass in jsonMasterMod.ItemClasses)
                {
                    ItemClass enumClass;
                    if (ItemClassEx.TryParse(itemClass, out enumClass))
                    {
                        _itemClasses.Add(enumClass);
                    }
                }
            }
            foreach (var spawnTagDict in jsonMod.SpawnTags)
            {
                foreach (var spawnTagPair in spawnTagDict)
                {
                    Tags tag;
                    if (TagsEx.TryParse(spawnTagPair.Key, out tag))
                    {
                        _spawnTags.Add(Tuple.Create(tag, spawnTagPair.Value));
                    }
                }
            }
            JsonMod = jsonMod;
            Stats = jsonMod.Stats.Select(s => new Stat(s)).ToList();
        }

        public bool Matches(ModDomain domain, Tags tags, ItemClass itemClass)
        {
            if (Domain != domain && !(domain == ModDomain.Item && Domain == ModDomain.Master))
            {
                return false;
            }
            if (_itemClasses.Contains(itemClass))
            {
                return true;
            }
            return (
                from spawnTag in _spawnTags
                where tags.HasFlag(spawnTag.Item1)
                select spawnTag.Item2
            ).FirstOrDefault();
        }
    }
}