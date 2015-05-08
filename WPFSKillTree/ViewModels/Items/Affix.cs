using MB.Algodat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace POESKillTree.ViewModels.Items
{
    public enum ModType
    {
        Prefix,
        Suffix
    }

    public class Affix
    {
        private static List<Affix> _AllAffixes = null;

        public static List<Affix> AllAffixes
        {
            get 
            {
                if (_AllAffixes == null)
                {
                    if (File.Exists(@"Data\Equipment\Itemlist.xml"))
                    {
                        XElement xelm = XElement.Load(@"Data\Equipment\Affixlist.xml");
                        _AllAffixes = xelm.Elements().Select(x => new Affix(x)).ToList();
                    }
                }
                return Affix._AllAffixes; 
            }
        }


        public HashSet<GearGroup> ApplicableGear { get; set; }
        public ModType ModType { get; set; }

        public bool IsSignatureMod { get; set; }
        public bool IsLegacyMod { get; set; }
        public bool IsTwoHandedWeaponMod { get; set; }
        public bool IsOneHandedWeaponMod { get; set; }
        public string Category { get; set; }

        public Affix(string[] mod, IEnumerable<ItemModTier> modlist)
        {
            this.Mod = mod;

            this.Tiers = new RangeTree<float, ModWrapper>[this.Mod.Length];

            foreach (var item in modlist)
                item.ParentAffix = this;

            for (int i = 0; i < Tiers.Length; i++)
                this.Tiers[i] = new RangeTree<float, ModWrapper>(modlist.Select(im => new ModWrapper(Mod[i], im)), new ItemModComparer());

            Aliases = Enumerable.Range(0, mod.Length).Select(_ => new HashSet<string>()).ToArray();
            ApplicableGear = new HashSet<GearGroup>();
            this.IsSignatureMod = false;
            this.Category = "";
        }

        private void ReCalculateTiers()
        {
            var arr = GetTiers().Reverse().ToArray();
            int tier = 1;
            for (int i = 0; i < arr.Length; i++)
            {
                if (!arr[i].IsMasterCrafted)
                {
                    arr[i].Tier = tier;
                    tier++;
                }
                else
                    arr[i].Tier = 0;
            }
        }

        public Affix(XElement e)
        {
            if (e.Name != "affix")
                throw new ArgumentException();

            this.Mod = e.Attribute("mod").Value.Split('|').ToArray();
            this.ModType = (ModType)Enum.Parse(typeof(ModType), e.Attribute("modtype").Value);

            if (e.Attribute("signaturemod") != null)
                this.IsSignatureMod = XmlConvert.ToBoolean(e.Attribute("signaturemod").Value);

            if (e.Attribute("legacymod") != null)
                this.IsLegacyMod = XmlConvert.ToBoolean(e.Attribute("legacymod").Value);

            this.Category = "";
            if (e.Attribute("category") != null)
                this.Category = e.Attribute("category").Value;

            if (e.Attribute("twohweaponmod") != null)
                this.IsTwoHandedWeaponMod = XmlConvert.ToBoolean(e.Attribute("twohweaponmod").Value);

            if (e.Attribute("onehweaponmod") != null)
                this.IsOneHandedWeaponMod = XmlConvert.ToBoolean(e.Attribute("onehweaponmod").Value);

            if (e.Attribute("applicablegear") != null && !string.IsNullOrEmpty(e.Attribute("applicablegear").Value))
                this.ApplicableGear = new HashSet<GearGroup>(e.Attribute("applicablegear").Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => (GearGroup)Enum.Parse(typeof(GearGroup), i)));
            else
                this.ApplicableGear = new HashSet<GearGroup>();

            this.Aliases = e.Element("aliases").Elements().Where(el => el.Name == "alias").Select(a => new HashSet<string>(a.Value.Split('|'))).ToArray();
            var tiers = e.Element("tiers").Elements().Select(el => new ItemModTier(el) { ParentAffix = this });

            this.Tiers = new RangeTree<float, ModWrapper>[this.Mod.Length];
            for (int i = 0; i < Tiers.Length; i++)
                this.Tiers[i] = new RangeTree<float, ModWrapper>(tiers.Select(im => new ModWrapper(Mod[i], im)), new ItemModComparer());
        }

        public XElement Serialize()
        {
            return new XElement("affix",
                    new XAttribute("mod", string.Join("|", this.Mod)),
                    new XAttribute("modtype", this.ModType),
                    new XAttribute("signaturemod", this.IsSignatureMod),
                    new XAttribute("legacymod", this.IsLegacyMod),
                    new XAttribute("category", this.Category),
                    new XAttribute("twohweaponmod", this.IsTwoHandedWeaponMod),
                    new XAttribute("onehweaponmod", this.IsOneHandedWeaponMod),
                    new XAttribute("applicablegear", string.Join(",", this.ApplicableGear.Select(g => g.ToString()).OrderBy(s => s))),
                    new XElement("aliases", Aliases.Select(a => new XElement("alias", string.Join("|", a)))),
                    new XElement("tiers", GetTiers().Select(m => m.Serialize()))
                );
        }

        public string[] Mod { get; set; }

        public HashSet<string>[] Aliases { get; set; }

        public string[] AliasStrings
        {
            get { return Aliases.Select(al =>String.Join(",", al.Select(a => a.Replace("minimum ", "").Replace("maximum ", "")).Distinct())).ToArray(); }
        }

        public string Name
        {
            get { return String.Join(",", AliasStrings.Distinct()); }
        }

        private RangeTree<float, ModWrapper>[] Tiers { get; set; }

        private class ModWrapper : IRangeProvider<float>
        {
            public string Mod { get; private set; }
            public ItemModTier ItemMod { get; private set; }

            public ModWrapper(string mod, ItemModTier imod)
            {
                this.Mod = mod;
                this.ItemMod = imod;
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

        public List<ItemModTier> QueryMod(string alias, float value)
        {
            for (int i = 0; i < Aliases.Length; i++)
                if (Aliases[i].Contains(alias))
                    return Tiers[i].Query(value).Select(mw => mw.ItemMod).ToList();

            throw new ArgumentException("alias not found");
        }

        public List<ItemModTier> QueryMod(int index, float value)
        {
            return Tiers[index].Query(value).Select(mw => mw.ItemMod).ToList();
        }

        public List<ItemModTier> QueryMod(string alias, Range<float> value)
        {
            for (int i = 0; i < Aliases.Length; i++)
                if (Aliases[i].Contains(alias))
                    return Tiers[i].Query(value).Select(mw => mw.ItemMod).ToList();

            throw new ArgumentException("alias not found");
        }

        public List<ItemModTier> Query(params float[] value)
        {
            if (Tiers.Length != value.Length)
                throw new ArgumentException("different number ov number than search params");
            var matches = value.Select((v, i) => Tiers[i].Query(v).Select(w => w.ItemMod).ToArray()).ToArray();

            return matches.Aggregate((a, n) => a.Intersect(n).ToArray()).ToList();
        }

        public List<ItemModTier> Query(params Range<float>[] range)
        {
            if (Tiers.Length != range.Length)
                throw new ArgumentException("different number of number than search params");

            return range.Select((v, i) => Tiers[i].Query(v).Select(w => w.ItemMod)).Aggregate((a, n) => a.Intersect(n)).ToList();
        }

        public ItemModTier[] GetTiers()
        {
            return Tiers[0].Items.Select(w => w.ItemMod).ToArray();
        }

        public override string ToString()
        {
            return "Mod(" + (ModType == ModType.Suffix ? "S" : "P") + "): " + string.Join("|", Mod);
        }

        public bool CanBeAppliedTo(Item item)
        {
            if (IsLegacyMod)
                return false;

            if (ApplicableGear.Count <= 0)
                return true;

            if (ApplicableGear.Contains(item.GearGroup))
            {
                if (item.IsWeapon)
                {
                    if (IsTwoHandedWeaponMod || IsOneHandedWeaponMod)
                    {
                        if (item.IsTwoHanded == IsTwoHandedWeaponMod)
                            return true;
                        else if (Category == "Two Handed Damage" && item.GearGroup == GearGroup.Bow)
                            return true;
                    }
                    else
                        return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public string GetModName(string alias)
        {
            for (int i = 0; i < Aliases.Length; i++)
                if (Aliases[i].Contains(alias))
                    return Mod[i];

            throw new ArgumentException("alias not found");
        }

        public bool IsPrefix
        {
            get { return this.ModType == Items.ModType.Prefix; }
        }

        public bool IsSuffix
        {
            get { return this.ModType == Items.ModType.Suffix; }
        }

        public float Minimum
        {
            get { return Tiers[0].Items.Min(i => i.Range.From); }
        }

        public float Maximum
        {
            get { return Tiers[0].Items.Max(i => i.Range.To); }
        }
    }
}
