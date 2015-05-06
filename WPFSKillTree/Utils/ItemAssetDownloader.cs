using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml.Linq;
using ItemClass = POESKillTree.ViewModels.ItemAttribute.Item.ItemClass;
using System.IO;
using POESKillTree.ViewModels.ItemAttribute;

namespace POESKillTree.Utils
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
                    if (File.Exists(@"Data\Equipment\Itemlist.xml"))
                    {
                        XElement xelm = XElement.Load(@"Data\Equipment\Itemlist.xml");
                        _baseList = xelm.Elements().Select(x => new ItemBase(x)).ToList();
                    }
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
                    nod = new WordSetTreeNode() { Word = word, Parent = this, _Level = this._Level+1 };
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

                    if (nod.Children== null || !nod.Children.TryGetValue(w, out nod))
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

                List<WordSetTreeNode> ms= new List<WordSetTreeNode>();

                for (int i = 0; i < wlist.Length; i++)
                {
                    var m= _root.Match(wlist.Skip(i)).OrderByDescending(n=>n.Level).FirstOrDefault();
                    if (m != null)
                        ms.Add(m);
                }

                if(ms.Count == 0)
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


        public string Group { get; set; }
        public int Level { get; set; }
        public string ItemType { get; set; }
        public ItemClass Class { get; set; }

        public int RequiredStr { get; set; }
        public int RequiredDex { get; set; }
        public int RequiredInt { get; set; }


        public List<ItemModRange> ImplicitMods { get; set; }
        public List<ItemModRange> Properties { get; set; }

        public ItemBase()
        {
            ImplicitMods = new List<ItemModRange>();
            Properties = new List<ItemModRange>();
        }

        public ItemBase(XElement x)
        {
            this.Group = x.Attribute("group").Value;
            this.Class = (ItemClass)Enum.Parse(typeof(ItemClass), x.Attribute("class").Value);
            this.ItemType = x.Attribute("type").Value;
            this.Level = int.Parse(x.Attribute("level").Value);

            if (x.Element("implicitmods") != null)
                ImplicitMods = x.Element("implicitmods").Elements().Select(e => new ItemModRange(e)).ToList();

            if (x.Element("properties") != null)
                Properties = x.Element("properties").Elements().Select(e => new ItemModRange(e)).ToList();
        }

        public virtual XElement Serialize()
        {
            var elm = new XElement("ItemBase",
                new XAttribute("group", Group),
                new XAttribute("class", Class),
                new XAttribute("type", ItemType),
                new XAttribute("level", Level));

            if (ImplicitMods.Count > 0)
                elm.Add(new XElement("implicitmods", ImplicitMods.Select(m => m.Serialize())));

            if (Properties.Count > 0)
                elm.Add(new XElement("properties", Properties.Select(m => m.Serialize())));


            return elm;
        }

        public class ItemModRange
        {
            public ItemModRange()
            { }

            public ItemModRange(XElement e)
            {
                this.Attribute = e.Attribute("attribute").Value;
                this.Value = e.Attribute("value").Value;
            }
            public string Attribute { get; set; }
            public string Value { get; set; }

            public XElement Serialize()
            {
                return new XElement("mod",
                        new XAttribute("attribute", Attribute),
                        new XAttribute("value", Value)
                    );
            }
        }

        public Item CreateItem()
        {
            var item = new Item();

            item.Class = this.Class;
            item.Type = this.ItemType;

            return item;
        }
    }

    internal class ItemAssetDownloader
    {

        static Dictionary<string, ItemClass> JewelryClassMap = new Dictionary<string, ItemClass>()
        {
            {"Amulet", ItemClass.Amulet},
            {"Belt",ItemClass.Belt},
            {"Ring", ItemClass.Ring},
        };


        static Dictionary<string, ItemClass> WeaponClassMap = new Dictionary<string, ItemClass>()
        {
            {"Bow",ItemClass.TwoHand},
            {"Claw",ItemClass.OneHand},
            {"Dagger",ItemClass.OneHand},
            {"One Hand Axe",ItemClass.OneHand},
            {"One Hand Mace",ItemClass.OneHand},
            {"One Hand Sword",ItemClass.OneHand},
            {"Sceptre",ItemClass.OneHand},
            {"Staff",ItemClass.TwoHand},
            {"Thrusting One Hand Sword",ItemClass.OneHand},
            {"Two Hand Axe",ItemClass.TwoHand},
            {"Two Hand Mace",ItemClass.TwoHand},
            {"Two Hand Sword",ItemClass.TwoHand},
            {"Wand",ItemClass.OneHand},
        };

        static Dictionary<string, ItemClass> ArmorClassMap = new Dictionary<string, ItemClass>()
        {
            {"Body Armour",ItemClass.Armor},
            {"Boots",ItemClass.Boots},
            {"Gloves",ItemClass.Gloves},
            {"Helmet",ItemClass.Helm},
            {"Shield",ItemClass.OffHand},
        };


        public static void ExtractJewelry(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/jewelry", "GET");

            var breaks = doc.DocumentNode.SelectNodes("//br");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i++)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;

                    var name = namel[1].InnerText;
                    var level = namel[2].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var xe = new ItemBase()
                    {
                        Group = h,
                        Class = JewelryClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level)

                    };

                    var implModnodes = new string[2][];
                    implModnodes[0] = namel[3].InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    implModnodes[1] = namel[4].InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).ToArray();


                    if (implModnodes.Length > 0)
                    {
                        for (int j = 0; j < implModnodes[0].Length; j++)
                        {
                            xe.ImplicitMods.Add(
                                new ItemBase.ItemModRange()
                                {
                                    Attribute = implModnodes[0][j],
                                    Value = implModnodes[1][j]
                                });
                        }
                    }


                    items.Add(xe);
                }
            }

        }

        public static void ExtractArmors(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/armour", "GET");

            var breaks = doc.DocumentNode.SelectNodes("//br");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i += 2)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                    var name = namel[1].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var level = namel[2].InnerText;


                    var armour = namel[3].InnerText;
                    var evasionrating = namel[4].InnerText;
                    var energyshield = namel[5].InnerText;

                    var reqStr = namel[6].InnerText;
                    var reqDex = namel[7].InnerText;
                    var reqInt = namel[8].InnerText;


                    var xe = new ItemBase()
                    {
                        Group = h,
                        Class = ArmorClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level),
                        RequiredStr = int.Parse(reqStr),
                        RequiredDex = int.Parse(reqDex),
                        RequiredInt = int.Parse(reqInt),
                    };


                    if (armour != "0")
                        xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Armour", Value = armour });
                    if (evasionrating != "0")
                        xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Energy Shield", Value = evasionrating });
                    if (energyshield != "0")
                        xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Evasion Rating", Value = energyshield });


                    var implModnodes = lines[i + 1].SelectNodes("td")
                        .Select(n => n.InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(v => v.Any(s => !string.IsNullOrEmpty(s))).ToArray();

                    if (implModnodes.Length > 0)
                    {
                        implModnodes[0] = implModnodes[0].Select(n => n.Replace("Minimum", "").Replace("Maximum", "").Trim()).Distinct().ToArray();

                        for (int j = 0; j < implModnodes[0].Length; j++)
                        {
                            xe.ImplicitMods.Add(
                                   new ItemBase.ItemModRange()
                                   {
                                       Attribute = implModnodes[0][j],
                                       Value = implModnodes[1][j]
                                   });
                        }
                    }


                    items.Add(xe);
                }
            }
        }

        public static void ExtractWeapons(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/weapon", "GET");


            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");


            XElement root = new XElement("itembaselist");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i += 2)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                    var name = namel[1].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var level = namel[2].InnerText;
                    var damage = namel[3].InnerText;
                    var aps = namel[4].InnerText;
                    var dps = namel[5].InnerText;
                    var reqStr = namel[6].InnerText;
                    var reqDex = namel[7].InnerText;
                    var reqInt = namel[8].InnerText;

                    var xe = new ItemBase()
                    {
                        Group = h,
                        Class = WeaponClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level),
                        RequiredStr = int.Parse(reqStr),
                        RequiredDex = int.Parse(reqDex),
                        RequiredInt = int.Parse(reqInt),
                    };



                    xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Physycal Damage", Value = damage });

                    xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Atacs Per Second", Value = aps });

                    xe.Properties.Add(new ItemBase.ItemModRange() { Attribute = "Critical Strike Chance", Value = "5%" });


                    var implModnodes = lines[i + 1].SelectNodes("td")
                        .Select(n => n.InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(v => v.Any(s => !string.IsNullOrEmpty(s))).ToArray();

                    if (implModnodes.Length > 0)
                    {
                        implModnodes[0] = implModnodes[0].Select(n => n.Replace("Minimum", "").Replace("Maximum", "").Trim()).Distinct().ToArray();

                        for (int j = 0; j < implModnodes[0].Length; j++)
                        {
                            xe.ImplicitMods.Add(
                                   new ItemBase.ItemModRange()
                                   {
                                       Attribute = implModnodes[0][j],
                                       Value = implModnodes[1][j]
                                   });
                        }
                    }

                    items.Add(xe);
                }
            }
        }
    }
}
