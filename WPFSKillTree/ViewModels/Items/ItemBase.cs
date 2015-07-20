using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace POESKillTree.ViewModels.Items
{

    public class ItemBase
    {
        static List<ItemBase> _baseList = null;
        public static List<ItemBase> BaseList
        {
            get
            {
                if (_baseList == null)
                {
                    var filename = Path.Combine(AppData.GetFolder(@"Data\Equipment"), @"Itemlist.xml");
                    if (File.Exists(filename))
                    {
                        XElement xelm = XElement.Load(filename);
                        _baseList = xelm.Elements().Select(x => new ItemBase(x)).ToList();
                    }
                    else
                        _baseList = new List<ItemBase>();
                }
                return ItemBase._baseList;
            }
        }


        private class WordSetTreeNode
        {

            private int _Level = 0;

            public int Level
            {
                get { return _Level; }
            }

            public string Word { get; set; }
            private WordSetTreeNode _Parent = null;

            public WordSetTreeNode Parent
            {
                get { return _Parent; }
                set { _Parent = value; }
            }

            private bool _IsLeaf = false;

            public bool IsLeaf
            {
                get { return _IsLeaf; }
                set { _IsLeaf = value; }
            }

            private Dictionary<string, WordSetTreeNode> _Children;

            public Dictionary<string, WordSetTreeNode> Children
            {
                get { return _Children; }
            }

            public void AddWordset(IEnumerable<string> words)
            {
                string word = words.First();
                if (_Children == null)
                    _Children = new Dictionary<string, WordSetTreeNode>();

                WordSetTreeNode nod = null;
                _Children.TryGetValue(word, out nod);

                if (nod == null)
                {
                    nod = new WordSetTreeNode() { Word = word, Parent = this, _Level = this._Level + 1 };
                    _Children.Add(word, nod);
                }

                if (words.Count() > 1)
                {
                    nod.AddWordset(words.Skip(1));
                }
                else
                    nod.IsLeaf = true;
            }

            internal IEnumerable<WordSetTreeNode> Match(IEnumerable<string> enumerable)
            {
                var nod = this;
                foreach (var w in enumerable)
                {
                    if (nod.IsLeaf)
                        yield return nod;

                    if (nod.Children == null || !nod.Children.TryGetValue(w, out nod))
                        yield break;

                }

                if (nod.IsLeaf)
                    yield return nod;
            }

            public override string ToString()
            {
                var lst = new Stack<string>();
                var nod = this;
                while (nod.Parent != null)
                {
                    lst.Push(nod.Word);
                    nod = nod.Parent;
                }

                return string.Join(" ", lst);
            }
        }

        static WordSetTreeNode _root = null;
        public static string ItemTypeFromTypeline(string typeline)
        {
            if (BaseList != null)
            {
                if (_root == null)
                {
                    _root = new WordSetTreeNode();
                    var allbs = BaseList.Select(m => m.ItemType.Split(' ')).OrderBy(s => s[0]).ToArray();
                    foreach (var wl in allbs)
                        _root.AddWordset(wl);
                }

                var wlist = typeline.Split(' ');

                List<WordSetTreeNode> ms = new List<WordSetTreeNode>();

                for (int i = 0; i < wlist.Length; i++)
                {
                    var m = _root.Match(wlist.Skip(i)).OrderByDescending(n => n.Level).FirstOrDefault();
                    if (m != null)
                        ms.Add(m);
                }

                if (ms.Count == 0)
                    return typeline;
                else if (ms.Count > 1)
                    throw new NotImplementedException("duplicate type match");
                else
                    return ms[0].ToString();
            }

            return typeline;
        }

        public static ItemClass ClassForItemType(string type)
        {
            try
            {
                var cls = BaseList.FirstOrDefault(b => b.ItemType == type);
                if (cls != null)
                    return cls.Class;
            }
            catch
            { }
            return ItemClass.Invalid;
        }

        public static int GetWidthForItem(ItemClass ics, GearGroup group, string type)
        {
            switch (group)
            {
                case GearGroup.Helmet:
                case GearGroup.Chest:
                case GearGroup.Belt:
                case GearGroup.Gloves:
                case GearGroup.Boots:
                case GearGroup.Axe:
                case GearGroup.Claw:
                case GearGroup.Bow:
                case GearGroup.Quiver:
                case GearGroup.Sceptre:
                case GearGroup.Staff:
                case GearGroup.Shield:
                    return 2;

                case GearGroup.Mace:
                    if (type.EndsWith("Club") || type == "Tenderizer")
                        return 1;
                    else return 2;

                case GearGroup.Sword:
                    if (ics == ItemClass.TwoHand)
                        if (type == "Corroded Blade")
                            return 1;
                        else
                            return 2;
                    else
                    {
                        if (type.EndsWith("Foil") || type.EndsWith("Rapier"))
                            return 1;

                        switch (type)
                        {
                            case "Pecoraro":
                            case "Foil":
                            case "Spike":
                            case "Gemstone Sword":
                            case "Corsair Sword":
                            case "Cutlass":
                            case "Variscite Blade":
                            case "Sabre":
                            case "Copper Sword":
                                return 1;
                        }

                        return 2;
                    }
            }

            return 1;
        }


        public static int GetHeightForItem(ItemClass ics, GearGroup group, string type)
        {
            switch (group)
            {
                case GearGroup.Staff:
                case GearGroup.Bow:
                    return 4;
                case GearGroup.Chest:
                    return 3;
                case GearGroup.Helmet:
                case GearGroup.Gloves:
                case GearGroup.Boots:
                case GearGroup.Claw:
                    return 2;

                case GearGroup.Axe:
                case GearGroup.Mace:
                case GearGroup.Sceptre:
                case GearGroup.Sword:
                    return (ics == ItemClass.TwoHand || type.EndsWith("Foil") || type.EndsWith("Rapier") || type == "Pecoraro" || type == "Foil" || type == "Spike" ) ? 4 : 3;

                case GearGroup.Shield:
                    if (type.EndsWith("Kite Shield") || type.EndsWith("Round Shield"))
                        return 3;
                    else if (type.EndsWith("Tower Shield"))
                        return 4;
                    else
                        return 2;

                case GearGroup.Quiver:
                case GearGroup.Dagger:
                case GearGroup.Wand:
                    return 3;
                case GearGroup.Flask:
                    return 2;
            }

            return 1;
        }


        public GearGroup GearGroup { get; set; }
        public int Level { get; set; }
        public string ItemType { get; set; }
        public ItemClass Class { get; set; }

        public int RequiredStr { get; set; }
        public int RequiredDex { get; set; }
        public int RequiredInt { get; set; }


        public List<Stat> ImplicitMods { get; set; }
        public List<Stat> Properties { get; set; }

        public ItemBase()
        {
            ImplicitMods = new List<Stat>();
            Properties = new List<Stat>();
        }

        public ItemBase(XElement x)
        {
            this.GearGroup = (GearGroup)Enum.Parse(typeof(GearGroup), x.Attribute("group").Value);
            this.Class = (ItemClass)Enum.Parse(typeof(ItemClass), x.Attribute("class").Value);
            this.ItemType = x.Attribute("type").Value;
            this.Level = int.Parse(x.Attribute("level").Value);

            if (x.Element("implicitmods") != null)
                ImplicitMods = x.Element("implicitmods").Elements().Select(e => new Stat(e)).ToList();

            if (x.Element("properties") != null)
                Properties = x.Element("properties").Elements().Select(e => new Stat(e)).ToList();
        }

        public virtual XElement Serialize()
        {
            var elm = new XElement("ItemBase",
                new XAttribute("group", GearGroup),
                new XAttribute("class", Class),
                new XAttribute("type", ItemType),
                new XAttribute("level", Level));

            if (ImplicitMods.Count > 0)
                elm.Add(new XElement("implicitmods", ImplicitMods.Select(m => m.Serialize())));

            if (Properties.Count > 0)
                elm.Add(new XElement("properties", Properties.Select(m => m.Serialize())));


            return elm;
        }

        public Item CreateItem()
        {
            var item = new Item();

            item.Class = this.Class;
            item.BaseType = this.ItemType;
            item.GearGroup = this.GearGroup;

            item.Width = GetWidthForItem(this.Class, this.GearGroup, this.ItemType);
            item.Height = GetHeightForItem(this.Class, this.GearGroup, this.ItemType);

            item.Properties = GetRawProperties();
            item.Keywords = GetKeywords();
            return item;
        }


        public List<string> GetKeywords()
        {
            if ((ItemClass.MainHand & this.Class) != 0 || this.Class == ItemClass.TwoHand)
            {
                List<string> props = new List<string>();

                if (this.Class == ItemClass.TwoHand && this.GearGroup != Items.GearGroup.Bow)
                    props.Add("Two Handed");

                props.Add("" + this.GearGroup);
                return props;
            }

            return null;
        }

        public List<ItemMod> GetRawProperties()
        {
            List<ItemMod> props = new List<ItemMod>();

            var kw = GetKeywords();
            if (kw != null && kw.Count > 0)
                props.Add(new ItemMod() { Attribute = string.Join(" ", kw) });

            if (Properties != null)
                foreach (var prop in Properties)
                {
                    props.Add(prop.ToItemMod());
                }
            return props;
        }


        internal static GearGroup GroupTypeForItemType(string type)
        {
            var cls = BaseList.FirstOrDefault(b => b.ItemType == type);

            if (cls != null)
                return cls.GearGroup;

            return GearGroup.Unknown;
        }
    }
}
