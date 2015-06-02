using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Linq;
using System;
using POESKillTree.Utils;
using POESKillTree.ViewModels.Items;
using Newtonsoft.Json.Linq;
using MB.Algodat;

namespace POESKillTree.ViewModels.Items
{
    public class Item : INotifyPropertyChanged, IRangeProvider<int>
    {
        /// <summary>
        /// itemclass and itemslot values with same name must have same value
        /// </summary>
        [Flags]
        public enum ItemSlot
        {
            Unequipable = 0x0,
            Armor = 0x1,
            MainHand = 0x2,
            OffHand = 0x4,
            Ring = 0x8,
            Ring2 = 0x10,
            Amulet = 0x20,
            Helm = 0x40,
            Gloves = 0x80,
            Boots = 0x100,
            Gem = 0x200,
            Belt = 0x400,
            TwoHand = 0x800,
        }

        private static Regex colorcleaner = new Regex("\\<.+?\\>");
        private static readonly Regex numberfilter = new Regex(@"[0-9]*\.?[0-9]+");
        private static readonly Regex numberfilter2 = new Regex(@"%\d|[0-9]*\.?[0-9]+");

        public Dictionary<string, List<float>> Attributes;
        private ItemClass _Class;

        public ItemClass Class
        {
            get { return _Class; }
            set { _Class = value; OnPropertyChanged("Class"); }
        }

        private ItemSlot _Slot;

        public ItemSlot Slot
        {
            get { return _Slot; }
            set { _Slot = value; OnPropertyChanged("Slot"); }
        }

        private GearGroup _geargroup;

        public GearGroup GearGroup
        {
            get { return _geargroup; }
            set { _geargroup = value; OnPropertyChanged("GearGroup"); }
        }


        public List<Item> Gems;
        private List<string> _Keywords;

        public List<string> Keywords
        {
            get { return _Keywords; }
            set { _Keywords = value; OnPropertyChanged("Keywords"); OnPropertyChanged("HaveKeywords"); }
        }

        public bool HaveKeywords
        {
            get { return Keywords != null && Keywords.Count > 0; }
        }


        private FrameType _frame;
        public FrameType Frame
        {
            get { return _frame; }
            set { _frame = value; OnPropertyChanged("Frame"); }
        }
        List<ItemMod> _properties = new List<ItemMod>();
        public List<ItemMod> Properties
        {
            get { return _properties; }
            set { _properties = value; OnPropertyChanged("Properties"); OnPropertyChanged("HaveProperties"); }
        }

        List<ItemMod> _requirements = new List<ItemMod>();
        public List<ItemMod> Requirements
        {
            get { return _requirements; }
            set { _requirements = value; OnPropertyChanged("Requirements"); OnPropertyChanged("HaveRequirements"); }
        }

        List<ItemMod> _implicitMods = new List<ItemMod>();
        public List<ItemMod> ImplicitMods
        {
            get { return _implicitMods; }
            set { _implicitMods = value; OnPropertyChanged("Implicitmods"); OnPropertyChanged("HaveImplicitMods"); }
        }

        List<ItemMod> _explicitMods = new List<ItemMod>();
        public List<ItemMod> ExplicitMods
        {
            get { return _explicitMods; }
            set { _explicitMods = value; OnPropertyChanged("ExplicitMods"); OnPropertyChanged("HaveExplicitMods"); }
        }

        List<ItemMod> _craftedMods = new List<ItemMod>();
        public List<ItemMod> CraftedMods
        {
            get { return _craftedMods; }
            set { _craftedMods = value; OnPropertyChanged("CraftedMods"); OnPropertyChanged("HaveCraftedMods"); }
        }


        public bool HaveProperties
        {
            get { return _properties.Count > 0; }
        }

        public bool HaveRequirements
        {
            get { return _requirements.Count > 0; }
        }

        public bool HaveImplicitMods
        {
            get { return _implicitMods.Count > 0; }
        }

        public bool HaveExplicitMods
        {
            get { return _explicitMods.Count > 0; }
        }

        public bool HaveCraftedMods
        {
            get { return _craftedMods.Count > 0; }
        }

        string _flavourText = null;

        public string FlavourText
        {
            get { return _flavourText; }
            set { _flavourText = value; OnPropertyChanged("FlavourText"); OnPropertyChanged("HaveFlavourText"); }
        }

        public bool HaveFlavourText
        {
            get { return !string.IsNullOrEmpty(_flavourText); }
        }


        public List<ItemMod> Mods;

        public string Name
        {
            get { return string.IsNullOrEmpty(_NameLine) ? TypeLine : NameLine; }
        }

        private string _NameLine;
        public string NameLine
        {
            get { return _NameLine; }
            set { _NameLine = value; OnPropertyChanged("NameLine"); OnPropertyChanged("HaveName"); }
        }

        private string _TypeLine;

        public string TypeLine
        {
            get { return _TypeLine; }
            set { _TypeLine = value; OnPropertyChanged("TypeLine"); }
        }

        public bool HaveName
        {
            get { return !string.IsNullOrEmpty(NameLine); }
        }

        // The socket group of gem (all gems with same socket group value are linked).
        public int SocketGroup;
        private string _Type;

        public string BaseType
        {
            get { return _Type; }
            set { _Type = value; OnPropertyChanged("Type"); }
        }


        public JObject JSONBase
        {
            get;
            set;
        }

        public Item(JObject val)
        {
            var cls = ItemClass.Unequipable;
            Init(cls, val);

            if (this.Frame < FrameType.Rare)
            {
                this.BaseType = ItemBase.ItemTypeFromTypeline(this.BaseType);
            }

            if (cls == ItemClass.Invalid || cls == ItemClass.Unequipable)
            {
                this.Class = ItemBase.ClassForItemType(this.BaseType);
            }

            this.GearGroup = ItemBase.GroupTypeForItemType(this.BaseType);

        }

        public Item(ItemClass iClass, JObject val)
        {
            Init(iClass, val);
        }

        public Item()
        {

        }

        public JObject GenerateJson()
        {
            var j = new JObject(
                new JProperty("w", W),
                new JProperty("h", H),
                new JProperty("x", X),
                new JProperty("y", Y),
                new JProperty("name", NameLine),
                new JProperty("typeLine", TypeLine),
                new JProperty("frameType", Frame)
                );

            if (Properties.Count > 0)
            {
                j.Add(new JProperty("properties",
                    new JArray(Properties.Select(p => p.ToJobject()).ToArray())));
            }

            if (Requirements.Count > 0)
            {
                j.Add(new JProperty("requirements",
                        new JArray(Requirements.Select(p => p.ToJobject()).ToArray())));
            }

            if (ImplicitMods.Count > 0)
            {
                j.Add(new JProperty("implicitMods",
                            new JArray(ImplicitMods.Select(p => p.ToJobject(true)).ToArray())));
            }

            if (ExplicitMods.Count > 0)
            {
                j.Add(new JProperty("explicitMods",
                            new JArray(ExplicitMods.Select(p => p.ToJobject(true)).ToArray())));
            }

            if (CraftedMods.Count > 0)
            {
                j.Add(new JProperty("craftedMods",
                            new JArray(CraftedMods.Select(p => p.ToJobject(true)).ToArray())));
            }

            if (HaveFlavourText)
                j.Add("flavourText",new JArray(FlavourText));

            return j;
        }

        private void Init(ItemClass iClass, JObject val)
        {
            JSONBase = val;

            Attributes = new Dictionary<string, List<float>>();
            Mods = new List<ItemMod>();
            Class = iClass;

            W = val["w"].Value<int>();
            H = val["h"].Value<int>();
            if (val["x"] != null)
                X = val["x"].Value<int>();
            if (val["y"] != null)
                Y = val["y"].Value<int>();

            if (val["name"] != null)
                NameLine = val["name"].Value<string>();

            TypeLine = BaseType = val["typeLine"].Value<string>();

            Frame = (FrameType)val["frameType"].Value<int>();

            if (val["properties"] != null)
                foreach (JObject obj in (JArray)val["properties"])
                {
                    var values = new List<float>();
                    string s = "";

                    foreach (JArray jva in (JArray)obj["values"])
                    {
                        s += " " + jva[0].Value<string>();
                    }
                    s = s.TrimStart();

                    if (s == "")
                    {
                        Properties.Add(ItemMod.CreateMod(this, obj["name"].Value<string>(), numberfilter));

                        Keywords = new List<string>();
                        string[] sl = obj["name"].Value<string>().Split(',');
                        foreach (string i in sl)
                            Keywords.Add(i.Trim());
                        continue;
                    }

                    foreach (Match m in numberfilter.Matches(s))
                    {
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                    }
                    string cs = obj["name"].Value<string>() + ": " + (numberfilter.Replace(s, "#"));

                    var mod = ItemMod.CreateMod(this, obj, numberfilter2);
                    Properties.Add(mod);


                    mod.ValueColor = ((JArray)obj["values"]).Select(a =>
                    {
                        var floats = ((JArray)a)[0].Value<string>().Split('-');
                        return floats.Select(f => (ItemMod.ValueColoring)((JArray)a)[1].Value<int>());
                    }).SelectMany(c => c).ToList();

                    Attributes.Add(cs, values);
                }

            if (val["requirements"] != null)
            {
                string reqs = "";
                List<float> numbers = new List<float>();
                List<ItemMod.ValueColoring> affects = new List<ItemMod.ValueColoring>();

                foreach (JObject obj in (JArray)val["requirements"])
                {
                    var n = obj["name"].Value<string>();

                    if (obj["displayMode"].Value<int>() == 0)
                        n = n + " #";
                    else
                        n = "# " + n;

                    numbers.Add(((JArray)((JArray)obj["values"])[0])[0].Value<float>());
                    affects.Add((ItemMod.ValueColoring)((JArray)((JArray)obj["values"])[0])[1].Value<int>());

                    if (!string.IsNullOrEmpty(reqs))
                        reqs += ", " + n;
                    else
                        reqs += n;
                }

                var m = ItemMod.CreateMod(this, "Requires " + reqs, numberfilter);
                m.Value = numbers;
                m.ValueColor = affects;
                Requirements.Add(m);
            }


            if (val["implicitMods"] != null)
                foreach (string s in val["implicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    ImplicitMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }
            if (val["explicitMods"] != null)
                foreach (string s in val["explicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    ExplicitMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }

            if (val["craftedMods"] != null)
                foreach (string s in val["craftedMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    CraftedMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }

            if (val["flavourText"] != null)
                FlavourText = string.Join("\r\n", val["flavourText"].Values<string>());


            if (iClass == ItemClass.Gem)
            {
                switch (val["colour"].Value<string>())
                {
                    case "S":
                        Keywords.Add("Strength");
                        break;

                    case "D":
                        Keywords.Add("Dexterity");
                        break;

                    case "I":
                        Keywords.Add("Intelligence");
                        break;
                }
            }
            else
            {
                Gems = new List<Item>();
            }

            var Sockets = new List<int>();
            if (val["sockets"] != null)
                foreach (JObject obj in (JArray)val["sockets"])
                {
                    Sockets.Add(obj["group"].Value<int>());
                }
            if (val["socketedItems"] != null)
            {
                int socket = 0;
                foreach (JObject obj in (JArray)val["socketedItems"])
                {
                    var item = new Item(ItemClass.Gem, obj);
                    item.SocketGroup = Sockets[socket++];
                    Gems.Add(item);
                }
            }
        }

        // Returns gems linked to specified gem.
        public List<Item> GetLinkedGems(Item gem)
        {
            var link = new List<Item>();

            foreach (Item linked in Gems)
                if (linked != gem && linked.SocketGroup == gem.SocketGroup)
                    link.Add(linked);

            return link;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }


        private int _x;

        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
                if (JSONBase != null)
                    JSONBase["x"] = value;
                OnPropertyChanged("X");
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                if (JSONBase != null)
                    JSONBase["y"] = value;
                OnPropertyChanged("Y");
            }
        }


        public int W { get; set; }
        public int H { get; set; }

        public bool IsWeapon
        {
            get { return this.Class == ItemClass.MainHand || this.Class == ItemClass.TwoHand; }
        }

        public bool IsTwoHanded
        {
            get { return this.Class == ItemClass.TwoHand; }
        }


        /// <summary>
        /// vertical range of item
        /// </summary>
        Range<int> IRangeProvider<int>.Range
        {
            get
            {
                return new Range<int>(Y, Y + H);
            }
        }
    }
}