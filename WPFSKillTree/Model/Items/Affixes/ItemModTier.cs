using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class ItemModTier : IEquatable<ItemModTier>
    {
        public string Name { get; }
        public IReadOnlyList<Stat> Stats { get; }
        public int Tier { get; }

        /// <summary>
        /// Constructor for a tier that is not more than a collection of stats.
        /// Used for implicit mods of an item.
        /// </summary>
        public ItemModTier(IEnumerable<Stat> stats)
        {
            Name = "";
            Stats = stats.Select(s => new Stat(s)).ToList();
        }

        public ItemModTier(XmlTier xmlTier, ItemType itemType)
        {
            Tier = xmlTier.Tier;
            Name = xmlTier.Name;
            Stats = xmlTier.Stats.Select(s => new Stat(s, itemType, xmlTier.ModGroup, xmlTier.ItemLevel)).ToList();
        }

        public override string ToString()
        {
            return Tier + " " + Name + " - " + string.Join("; ", Stats);
        }

        public bool Equals(ItemModTier other)
        {
            var x = this;
            var y = other;
            return x.Name == y.Name
                && x.Stats.Zip(y.Stats, (xs, ys) => xs.Equals(ys)).All(z => z);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode()
                ^ Stats.Aggregate(0, (a, s) => a ^ s.GetHashCode());
        }

    }

}
