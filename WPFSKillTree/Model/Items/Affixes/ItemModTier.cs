using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class ItemModTier : IEquatable<ItemModTier>
    {
        public string Name { get; }
        public int Level { get; }
        public IReadOnlyList<Stat> Stats { get; }
        public bool IsMasterCrafted { get; }
        public int Tier { get; }

        /// <summary>
        /// Constructor for a tier that is not more than a collection of stats.
        /// Used for implicit mods of an item.
        /// </summary>
        public ItemModTier(IEnumerable<Stat> stats)
        {
            Name = "";
            Stats = stats.Select(s => new Stat(s, this)).ToList();
        }

        public ItemModTier(XmlTier xmlTier, ItemType itemType)
        {
            IsMasterCrafted = xmlTier.IsMasterCrafted;
            Tier = xmlTier.Tier;
            Name = xmlTier.Name;
            Level = xmlTier.ItemLevel;
            Stats = xmlTier.Stats.Select(s => new Stat(s, itemType, this)).ToList();
        }

        public override string ToString()
        {
            return Tier + " " + Name + " - " + string.Join("; ", Stats);
        }

        public bool Equals(ItemModTier other)
        {
            var x = this;
            var y = other;
            return x.Level == y.Level && x.Name == y.Name && x.IsMasterCrafted == y.IsMasterCrafted
                && x.Stats.Zip(y.Stats, (xs, ys) => xs.Equals(ys)).All(z => z);
        }

        public override int GetHashCode()
        {
            var obj = this;
            return obj.Level.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.IsMasterCrafted.GetHashCode()
                ^ obj.Stats.Aggregate(0, (a, s) => a ^ s.GetHashCode());
        }

    }

}
