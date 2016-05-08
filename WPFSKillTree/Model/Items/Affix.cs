using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MB.Algodat;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class Affix
    {
        public static readonly IReadOnlyDictionary<ItemType, IReadOnlyList<Affix>> AffixesPerItemType;

        static Affix()
        {
            IReadOnlyList<Affix> allAffixes;
            var filename = Path.Combine(AppData.GetFolder(@"Data\Equipment"), @"AffixList.xml");
            if (File.Exists(filename))
            {
                using (var reader = new StreamReader(filename))
                {
                    var ser = new XmlSerializer(typeof(XmlAffixList));
                    var xmlList = (XmlAffixList) ser.Deserialize(reader);
                    allAffixes = xmlList.Affixes.Select(x => new Affix(x)).ToList();
                }
            }
            else
            {
                allAffixes = new List<Affix>();
            }
            
            AffixesPerItemType =
                (from a in allAffixes
                 group a by a._itemType into types
                 select types)
                 .ToDictionary(g => g.Key, g => (IReadOnlyList<Affix>) new List<Affix>(g));
        }

        private readonly ItemType _itemType;

        public ModType ModType { get; private set; }

        public bool IsRangeMod
        {
            get
            {
                return Mods.Select(s => s.Replace("minimum", "").Replace("maximum", "")).Distinct().Count() == 1;
            }
        }

        public IReadOnlyList<string> Mods { get; private set; }

        public string Name { get; private set; }

        private RangeTree<float, ModWrapper>[] Tiers { get; set; }

        public Affix(IEnumerable<string> mod, IReadOnlyList<ItemModTier> modlist)
        {
            Mods = mod.ToList();
            Name = string.Join(",", Mods);

            if (modlist.Any())
            {
                Tiers = new RangeTree<float, ModWrapper>[Mods.Count];

                foreach (var item in modlist)
                    item.ParentAffix = this;

                for (int i = 0; i < Tiers.Length; i++)
                    Tiers[i] = new RangeTree<float, ModWrapper>(modlist.Select(im => new ModWrapper(Mods[i], im)), new ItemModComparer());
            }
            _itemType = ItemType.Unknown;
        }

        private Affix(XmlAffix xmlAffix)
        {
            _itemType = xmlAffix.ItemType;
            ModType = xmlAffix.ModType;
            if (xmlAffix.Tiers.Any())
                Mods = xmlAffix.Tiers[0].Stats.Select(s => s.Name).ToList();
            else
                throw new NotSupportedException("There should not be any Affix without tiers");
            Name = xmlAffix.Name;

            var tiers = xmlAffix.Tiers.Select(el => new ItemModTier(el) { ParentAffix = this }).ToList();
            Tiers = new RangeTree<float, ModWrapper>[Mods.Count];
            for (var i = 0; i < Tiers.Length; i++)
                Tiers[i] = new RangeTree<float, ModWrapper>(tiers.Select(im => new ModWrapper(Mods[i], im)), new ItemModComparer());
        }

        public IEnumerable<ItemModTier> QueryMod(int index, float value)
        {
            return Tiers[index].Query(value).Select(mw => mw.ItemMod).ToList();
        }

        public IEnumerable<ItemModTier> Query(params float[] value)
        {
            if (Tiers.Length != value.Length)
                throw new ArgumentException("different number ov number than search params");
            var matches = value.Select((v, i) => Tiers[i].Query(v).Select(w => w.ItemMod).ToArray()).ToArray();

            return matches.Aggregate((a, n) => a.Intersect(n).ToArray()).OrderBy(c => c.IsMasterCrafted ? 1 : 0).ToList();
        }

        public ItemModTier[] GetTiers()
        {
            if (Tiers == null)
                return new ItemModTier[0];
            return Tiers[0].Items.Select(w => w.ItemMod).ToArray();
        }

        public override string ToString()
        {
            return "Mod(" + (ModType == ModType.Suffix ? "S" : "P") + "): " + string.Join(",", Mods);
        }

        private class ModWrapper : IRangeProvider<float>
        {
            private string Mod { get; set; }
            public ItemModTier ItemMod { get; private set; }

            public ModWrapper(string mod, ItemModTier imod)
            {
                Mod = mod;
                ItemMod = imod;
            }

            public Range<float> Range
            {
                get { return ItemMod.Range(Mod); }
            }

            public override string ToString()
            {
                return "ItemMod: " + Mod + " {" + ItemMod.Range(Mod) + "}";
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
