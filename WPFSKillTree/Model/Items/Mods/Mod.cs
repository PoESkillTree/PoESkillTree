using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PoESkillTree.GameModel.Items;
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
        /// <summary>
        /// Mod groups no mod with ModDomain.Master should match. These do not work well with crafting
        /// (multiple different stats with same value ranges) and are covered by normal mods anyway.
        /// </summary>
        private static readonly ISet<string> IgnoredMasterCraftedGroups = new HashSet<string>
        {
            "DefencesPercent", "BaseLocalDefences"
        };

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
        public Mod(string id, JsonMod jsonMod, IEnumerable<JsonCraftingBenchOption> jsonBenchOptions)
        {
            Id = id;
            foreach (var jsonMasterMod in jsonBenchOptions)
            {
                foreach (var itemClass in jsonMasterMod.ItemClasses)
                {
                    if (ItemClassEx.TryParse(itemClass, out ItemClass enumClass))
                    {
                        _itemClasses.Add(enumClass);
                    }
                }
            }
            foreach (var spawnWeight in jsonMod.SpawnWeights)
            {
                if (TagsExtensions.TryParse(spawnWeight.Tag, out Tags tag))
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
                if (Domain != ModDomain.Misc)
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
                if (Domain != ModDomain.Item && Domain != ModDomain.Crafted)
                {
                    return false;
                }
            }

            if (!IgnoredMasterCraftedGroups.Contains(JsonMod.Group) && _itemClasses.Contains(itemClass))
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