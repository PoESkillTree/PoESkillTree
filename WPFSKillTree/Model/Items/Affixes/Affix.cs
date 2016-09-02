using System;
using System.Collections.Generic;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.Model.Items.Affixes
{
    public class Affix
    {
        public ItemType ItemType { get; private set; }

        public ModType ModType { get; private set; }

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

                for (int i = 0; i < Tiers.Length; i++)
                    Tiers[i] = new RangeTree<float, ModWrapper>(modlist.Select(im => new ModWrapper(Mods[i], im)), new ItemModComparer());
            }
            ItemType = ItemType.Unknown;
        }

        public Affix(XmlAffix xmlAffix)
        {
            ItemType = xmlAffix.ItemType;
            ModType = xmlAffix.ModType;
            if (xmlAffix.Tiers.Any())
                Mods = xmlAffix.Tiers[0].Stats.Select(s => s.Name).ToList();
            else
                throw new NotSupportedException("There should not be any Affix without tiers");
            Name = xmlAffix.Name;

            var tiers = xmlAffix.Tiers.Select(el => new ItemModTier(el, ItemType)).ToList();
            Tiers = new RangeTree<float, ModWrapper>[Mods.Count];
            for (var i = 0; i < Tiers.Length; i++)
                Tiers[i] = new RangeTree<float, ModWrapper>(tiers.Select(im => new ModWrapper(Mods[i], im)), new ItemModComparer());
        }

        public IEnumerable<ItemModTier> QueryMod(int index, float value)
        {
            return Tiers[index].Query(value).Select(mw => mw.ItemMod);
        }

        public IEnumerable<ItemModTier> Query(params float[] value)
        {
            if (Tiers.Length != value.Length)
                throw new ArgumentException("different number ov number than search params");
            var matches = value.Select((v, i) => Tiers[i].Query(v).Select(w => w.ItemMod));

            return matches.Aggregate((a, n) => a.Intersect(n)).OrderByDescending(c => c.Tier);
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
