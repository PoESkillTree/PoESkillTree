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


        public GearGroup GearGroup { get; set; }
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
            this.GearGroup = (GearGroup)Enum.Parse(typeof(GearGroup), x.Attribute("group").Value);
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
            item.BaseType = this.ItemType;
            item.GearGroup = this.GearGroup;

            return item;
        }


        internal static GearGroup GroupTypeForItemType(string type)
        {
            var cls = BaseList.FirstOrDefault(b => b.ItemType == type);

            if(cls!=null)
                return cls.GearGroup;

            return GearGroup.Unknown;
        }
    }
}
