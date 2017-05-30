using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// Encapsulates a <see cref="JsonMod"/> into the <see cref="IMod"/> interface
    /// and provides a method to determine whether the mod can be crafted onto an item.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Mod : IMod
    {
        public string Id { get; }

        private readonly ISet<ItemClass> _itemClasses = new HashSet<ItemClass>();
        private readonly IList<Tuple<Tags, bool>> _spawnTags = new List<Tuple<Tags, bool>>();

        public JsonMod JsonMod { get; }

        public IReadOnlyList<IStat> Stats { get; }
        public string Name => JsonMod.Name;
        public ModDomain Domain => JsonMod.Domain;
        public int RequiredLevel => JsonMod.RequiredLevel;

        /// <param name="id">the id of this mod</param>
        /// <param name="jsonMod">the <see cref="JsonMod"/> to encapsulate</param>
        /// <param name="jsonBenchOptions">the master crafting options with which this mod can be crafted</param>
        /// <param name="spawnWeightsReplacement">replacement spawn weights if this mod can only be spawned by different 
        /// means, e.g. as a master signature mod</param>
        public Mod(string id, JsonMod jsonMod, IEnumerable<JsonCraftingBenchOption> jsonBenchOptions,
            IEnumerable<JsonSpawnWeight> spawnWeightsReplacement)
        {
            Id = id;
            foreach (var jsonMasterMod in jsonBenchOptions)
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
            var spawnWeights = spawnWeightsReplacement ?? jsonMod.SpawnWeights;
            foreach (var spawnWeight in spawnWeights)
            {
                Tags tag;
                if (TagsEx.TryParse(spawnWeight.Tag, out tag))
                {
                    _spawnTags.Add(Tuple.Create(tag, spawnWeight.CanSpawn));
                }
            }
            JsonMod = jsonMod;
            Stats = jsonMod.Stats.Select(s => new Stat(s)).ToList();
        }

        /// <returns>true if this mod can be crafted onto an item with the given tags and class</returns>
        public bool Matches(Tags tags, ItemClass itemClass)
        {
            // the ModDomains Item and Master match everything but Flask, Jewel and Gem
            if (tags.HasFlag(Tags.Flask))
            {
                if (Domain != ModDomain.Flask)
                {
                    return false;
                }
            }
            else if (tags.HasFlag(Tags.Jewel))
            {
                if (Domain != ModDomain.Jewel)
                {
                    return false;
                }
            }
            else if (tags.HasFlag(Tags.Gem))
            {
                return false;
            }
            else
            {
                if (Domain != ModDomain.Item && Domain != ModDomain.Master)
                {
                    return false;
                }
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