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
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
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
#if (PoESkillTree_UseSmallDec_ForAttributes)
        private readonly IReadOnlyList<Range<SmallDec>>[][] _ranges;

        private readonly IRangeTree<SmallDec, ModWrapper>[][] _trees;
#else
        private readonly IReadOnlyList<Range<float>>[][] _ranges;

        private readonly IRangeTree<float, ModWrapper>[][] _trees;
#endif

        private readonly IReadOnlyList<Stat> _firstTierStats;

        public Affix(params ItemModTier[] tiers)
            : this(ItemType.Unknown, tiers)
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
#if (PoESkillTree_UseSmallDec_ForAttributes)
            _trees = new IRangeTree<SmallDec, ModWrapper>[statCount][];
            _ranges = new IReadOnlyList<Range<SmallDec>>[statCount][];
#else
            _trees = new IRangeTree<float, ModWrapper>[statCount][];
            _ranges = new IReadOnlyList<Range<float>>[statCount][];
#endif
            var valueCounts = new int[statCount];
            for (int i = 0; i < _firstTierStats.Count; i++)
            {
                var stat = _firstTierStats[i];
                int rangeCount = stat.Ranges.Count;
                mods.Add(stat.Name);
#if (PoESkillTree_UseSmallDec_ForAttributes)
                _trees[i] = new IRangeTree<SmallDec, ModWrapper>[rangeCount];
                _ranges[i] = new IReadOnlyList<Range<SmallDec>>[rangeCount];
#else
                _trees[i] = new IRangeTree<float, ModWrapper>[rangeCount];
                _ranges[i] = new IReadOnlyList<Range<float>>[rangeCount];
#endif
                valueCounts[i] = rangeCount;

                if (tierList.Any(t => t.Stats[i].Ranges.Count != rangeCount))
                {
                    throw new NotSupportedException(
                        $"Tiers of stat {stat.Name} must all have the same amount of ranges");
                }

                for (int j = 0; j < rangeCount; j++)
                {
                    var wrapper = tierList.Select(t => new ModWrapper(t, t.Stats[i].Ranges[j])).ToList();
#if (PoESkillTree_UseSmallDec_ForAttributes)
                    _trees[i][j] = new RangeTree<SmallDec, ModWrapper>(wrapper, comparer);
#else
                    _trees[i][j] = new RangeTree<float, ModWrapper>(wrapper, comparer);
#endif
                    _ranges[i][j] = wrapper.Select(w => w.Range).ToList();
                }
            }

            StatNames = mods;
            ValueCountPerStat = valueCounts;
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
        public IEnumerable<ItemModTier> QueryMod(int statIndex, int valueIndex, SmallDec value)
#else
        public IEnumerable<ItemModTier> QueryMod(int statIndex, int valueIndex, float value)
#endif
        {
            return _trees[statIndex][valueIndex].Query(value).Select(mw => mw.ItemModTier);
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
        public IEnumerable<ItemModTier> Query(IEnumerable<IEnumerable<SmallDec>> values)
#else
        public IEnumerable<ItemModTier> Query(IEnumerable<IEnumerable<float>> values)
#endif
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

#if (PoESkillTree_UseSmallDec_ForAttributes)
        private static IEnumerable<ItemModTier> QueryForTree(SmallDec value, IRangeTree<SmallDec, ModWrapper> tree)
#else
        private static IEnumerable<ItemModTier> QueryForTree(float value, IRangeTree<float, ModWrapper> tree)
#endif
            => tree.Query(value).Select(w => w.ItemModTier);

#if (PoESkillTree_UseSmallDec_ForAttributes)
        public IEnumerable<ItemMod> ToItemMods(IEnumerable<IEnumerable<SmallDec>> values)
#else
        public IEnumerable<ItemMod> ToItemMods(IEnumerable<IEnumerable<float>> values)
#endif
        {
            return values.EquiZip(_firstTierStats, (vs, stat) => stat.ToItemMod(vs.ToList()));
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
        public IReadOnlyList<Range<SmallDec>> GetRanges(int statIndex, int valueIndex)
#else
        public IReadOnlyList<Range<float>> GetRanges(int statIndex, int valueIndex)
#endif
            => _ranges[statIndex][valueIndex];

        public override string ToString()
        {
            return "Mod(" + (ModType == ModType.Suffix ? "S" : "P") + "): " + Name;
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
        private class ModWrapper : IRangeProvider<SmallDec>
#else
        private class ModWrapper : IRangeProvider<float>
#endif
        {

            public ItemModTier ItemModTier { get; }

#if (PoESkillTree_UseSmallDec_ForAttributes)
            public Range<SmallDec> Range { get; }

            public ModWrapper(ItemModTier tier, Range<SmallDec> range)
#else
            public Range<float> Range { get; }

            public ModWrapper(ItemModTier tier, Range<float> range)
#endif
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
