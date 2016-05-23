using System;
using System.Collections.Generic;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class ItemModTier : IEquatable<ItemModTier>
    {
        public string Name { get; private set; }
        private int Level { get; set; }
        public IReadOnlyList<Stat> Stats { get; private set; }
        public bool IsMasterCrafted { get; private set; }
        public int Tier { get; private set; }

        public bool IsRangeMod
        {
            get { return RangeCombinedStat != null; }
        }

        public Stat RangeCombinedStat { get; private set; }

        /// <summary>
        /// Constructor for a tier that is not more than a collection of stats.
        /// Used for implicit mods of an item.
        /// </summary>
        public ItemModTier(IEnumerable<Stat> stats)
        {
            Name = "";
            Stats = stats.Select(s => new Stat(s.Name, s.Range, s.ItemType, this)).ToList();
            RangeCombinedStat = CreateRangeCombinedStat();
        }

        public ItemModTier(XmlTier xmlTier, ItemType itemType)
        {
            IsMasterCrafted = xmlTier.IsMasterCrafted;
            Tier = xmlTier.Tier;
            Name = xmlTier.Name;
            Level = xmlTier.ItemLevel;
            Stats = xmlTier.Stats.Select(s => new Stat(s, itemType, this)).ToList();
            RangeCombinedStat = CreateRangeCombinedStat();
        }

        private Stat CreateRangeCombinedStat()
        {
            if (Stats.Count < 2)
                return null;
            var names =
                Stats.Select(s => s.Name)
                    .Select(s => s.Replace(" minimum", "").Replace(" maximum", ""))
                    .Distinct().ToList();
            if (names.Count != 1)
                return null;
            return new Stat(names.Single(), new Range<float>(), Stats[0].ItemType, this);
        }

        public Range<float> Range(string mod)
        {
            return Stats.First(s => mod.Contains(s.Name)).Range;
        }

        public override string ToString()
        {
            return Tier + " " + Name + " - " + string.Join("; ", Stats.Select(s => s.Name + " {" + s.Range + "} "));
        }

        public bool Equals(ItemModTier other)
        {
            var x = this;
            var y = other;
            return x.Level == y.Level && x.Name == y.Name && x.IsMasterCrafted == y.IsMasterCrafted
                && x.Stats.Zip(y.Stats, (xs, ys) => xs.Name == ys.Name && xs.Range.CompareTo(ys.Range) == 0).All(z => z);
        }

        public override int GetHashCode()
        {
            var obj = this;
            return obj.Level.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.IsMasterCrafted.GetHashCode()
                ^ obj.Stats.Aggregate(0, (a, s) => a ^ s.Name.GetHashCode() ^ s.Range.GetHashCode());
        }

    }

}
