using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MB.Algodat;
using MoreLinq;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items.Affixes
{
    public class Affix
    {
        public ItemType ItemType { get; }

        public ModType ModType { get; }

        public IReadOnlyList<string> StatNames { get; }
        public IReadOnlyList<int> ValueCountPerStat { get; }

        [UsedImplicitly(ImplicitUseKindFlags.Access)] // accessed in ModSelectorView
        public string Name { get; }

        /// <summary>
        /// The ranges of this affix. The first array index specifies the stat (the stat's name is the element in
        /// StatNames with the same index). The second array index specifies the value of the stat. The list then
        /// specifies the ranges of the different tiers for this value of this stat.
        /// </summary>
        private readonly IReadOnlyList<Range<float>>[][] _ranges;

        private readonly IRangeTree<float, ModWrapper>[][] _trees;

        private readonly IReadOnlyList<Stat> _firstTierStats;

        public Affix()
            : this(ItemType.Unknown, new ItemModTier[0])
        {
            Name = "";
        }

        public Affix(ItemModTier tier)
            : this(ItemType.Unknown, new[] { tier })
        {
            Name = string.Join(",", StatNames);
        }

        public Affix(XmlAffix xmlAffix)
            : this(xmlAffix.ItemType, xmlAffix.Tiers.Select(el => new ItemModTier(el, xmlAffix.ItemType)))
        {
            if (!xmlAffix.Tiers.Any())
                throw new NotSupportedException("There should not be any Affix without tiers");
            ModType = xmlAffix.ModType;
            Name = xmlAffix.Name;
        }

        private Affix(ItemType itemType, IEnumerable<ItemModTier> tiers)
        {
            ItemType = itemType;
            var tierList = tiers.ToList();

            if (!tierList.Any())
            {
                StatNames = new string[0];
                _firstTierStats = new Stat[0];
                return;
            }

            var firstTier = tierList[0];
            _firstTierStats = firstTier.Stats;
            var statCount = _firstTierStats.Count;
            if (tierList.Any(t => t.Stats.Count != statCount))
            {
                throw new NotSupportedException("Tiers must all have the same amount of stats");
            }

            var comparer = new ItemModComparer();
            var mods = new List<string>();
            _trees = new IRangeTree<float, ModWrapper>[statCount][];
            _ranges = new IReadOnlyList<Range<float>>[statCount][];
            var valueCounts = new int[statCount];
            for (int i = 0; i < _firstTierStats.Count; i++)
            {
                var stat = _firstTierStats[i];
                int rangeCount = stat.Ranges.Count;
                mods.Add(stat.Name);
                _trees[i] = new IRangeTree<float, ModWrapper>[rangeCount];
                _ranges[i] = new IReadOnlyList<Range<float>>[rangeCount];
                valueCounts[i] = rangeCount;

                if (tierList.Any(t => t.Stats[i].Ranges.Count != rangeCount))
                {
                    throw new NotSupportedException(
                        $"Tiers of stat {stat.Name} must all have the same amount of ranges");
                }

                for (int j = 0; j < rangeCount; j++)
                {
                    var wrapper = tierList.Select(t => new ModWrapper(t, t.Stats[i].Ranges[j])).ToList();
                    _trees[i][j] = new RangeTree<float, ModWrapper>(wrapper, comparer);
                    _ranges[i][j] = wrapper.Select(w => w.Range).ToList();
                }
            }
            StatNames = mods;
            ValueCountPerStat = valueCounts;
        }

        public IEnumerable<ItemModTier> QueryMod(int statIndex, int valueIndex, float value)
        {
            return _trees[statIndex][valueIndex].Query(value).Select(mw => mw.ItemModTier);
        }

        public IEnumerable<ItemModTier> Query(IEnumerable<IEnumerable<float>> values)
        {
            if (!_trees.Any())
            {
                return Enumerable.Empty<ItemModTier>();
            }
            // for each value for each stat: query matching tiers
            return values.EquiZip(_trees, (vs, trees) => vs.EquiZip(trees, QueryForTree))
            // flatten values for stats
                .Flatten()
            // aggregate to keep only tiers that match all values of all stats
                .Aggregate((a, n) => a.Intersect(n))
                .OrderByDescending(t => t.Tier);
        }

        private static IEnumerable<ItemModTier> QueryForTree(float value, IRangeTree<float, ModWrapper> tree)
            => tree.Query(value).Select(w => w.ItemModTier);

        public IEnumerable<ItemMod> ToItemMods(IEnumerable<IEnumerable<float>> values)
        {
            return values.EquiZip(_firstTierStats, (vs, stat) => stat.ToItemMod(vs.ToList()));
        }

        public IReadOnlyList<Range<float>> GetRanges(int statIndex, int valueIndex)
            => _ranges[statIndex][valueIndex];

        public override string ToString()
        {
            return "Mod(" + (ModType == ModType.Suffix ? "S" : "P") + "): " + Name;
        }

        private class ModWrapper : IRangeProvider<float>
        {

            public ItemModTier ItemModTier { get; }

            public Range<float> Range { get; }

            public ModWrapper(ItemModTier tier, Range<float> range)
            {
                Range = range;
                ItemModTier = tier;
            }
        }

        private class ItemModComparer : IComparer<ModWrapper>
        {
            public int Compare(ModWrapper x, ModWrapper y)
            {
                return x.Range.CompareTo(y.Range);
            }
        }
    }
}
