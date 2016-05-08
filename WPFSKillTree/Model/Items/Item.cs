using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MB.Algodat;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class Item : Notifier, IRangeProvider<int>
    {
        private static readonly Regex Numberfilter = new Regex(@"[0-9]*\.?[0-9]+");
        private static readonly Regex Numberfilter2 = new Regex(@"%\d|[0-9]*\.?[0-9]+");

        public Dictionary<string, List<float>> Attributes { get; set; }

        private readonly ItemClass _class;
        public ItemClass Class
        {
            get { return _class; }
        }

        private ItemSlot _slot;
        public ItemSlot Slot
        {
            get { return _slot; }
            set { SetProperty(ref _slot, value); }
        }

        private readonly ItemType _itemType;
        public ItemType ItemType
        {
            get { return _itemType; }
        }

        private readonly ItemGroup _itemGroup;
        public ItemGroup ItemGroup
        {
            get { return _itemGroup; }
        }

        private readonly List<Item> _gems;
        public IReadOnlyList<Item> Gems
        {
            get { return _gems; }
        }

        private readonly List<string> _keywords;
        public IReadOnlyList<string> Keywords
        {
            get { return _keywords; }
        }

        private FrameType _frame;
        public FrameType Frame
        {
            get { return _frame; }
            set { SetProperty(ref _frame, value); }
        }

        private List<ItemMod> _properties = new List<ItemMod>();
        public List<ItemMod> Properties
        {
            get { return _properties; }
            set { SetProperty(ref _properties, value, () => OnPropertyChanged("HaveProperties")); }
        }
        public bool HaveProperties
        {
            get { return _properties.Count > 0; }
        }

        private readonly List<ItemMod> _requirements = new List<ItemMod>();
        public IReadOnlyList<ItemMod> Requirements
        {
            get { return _requirements; }
        }
        public bool HaveRequirements
        {
            get { return _requirements.Count > 0; }
        }

        private List<ItemMod> _implicitMods = new List<ItemMod>();
        public List<ItemMod> ImplicitMods
        {
            get { return _implicitMods; }
            set { SetProperty(ref _implicitMods, value, () => OnPropertyChanged("HaveImplicitMods")); }
        }
        public bool HaveImplicitMods
        {
            get { return _implicitMods.Count > 0; }
        }

        private List<ItemMod> _explicitMods = new List<ItemMod>();
        public List<ItemMod> ExplicitMods
        {
            get { return _explicitMods; }
            set { SetProperty(ref _explicitMods, value, () => OnPropertyChanged("HaveExplicitMods")); }
        }
        public bool HaveExplicitMods
        {
            get { return _explicitMods.Count > 0; }
        }

        private List<ItemMod> _craftedMods = new List<ItemMod>();
        public List<ItemMod> CraftedMods
        {
            get { return _craftedMods; }
            set { SetProperty(ref _craftedMods, value, () => OnPropertyChanged("HaveCraftedMods")); }
        }
        public bool HaveCraftedMods
        {
            get { return _craftedMods.Count > 0; }
        }

        private string _flavourText;
        public string FlavourText
        {
            get { return _flavourText; }
            set { SetProperty(ref _flavourText, value, () => OnPropertyChanged("HaveFlavourText")); }
        }
        public bool HaveFlavourText
        {
            get { return !string.IsNullOrEmpty(_flavourText); }
        }

        public List<ItemMod> Mods { get; set; }

        public string Name
        {
            get { return string.IsNullOrEmpty(_nameLine) ? TypeLine : NameLine; }
        }

        private string _nameLine;
        public string NameLine
        {
            get { return _nameLine; }
            set { SetProperty(ref _nameLine, value, () => OnPropertyChanged("HaveName")); }
        }
        public bool HaveName
        {
            get { return !string.IsNullOrEmpty(NameLine); }
        }

        private string _typeLine;
        public string TypeLine
        {
            get { return _typeLine; }
            set { SetProperty(ref _typeLine, value); }
        }

        // The socket group of gem (all gems with same socket group value are linked).
        private int _socketGroup;

        private readonly ItemBase _baseType;
        public ItemBase BaseType
        {
            get { return _baseType; }
        }
        
        public JObject JsonBase { get; private set; }

        private int _x;
        public int X
        {
            get { return _x; }
            set
            {
                SetProperty(ref _x, value, () =>
                {
                    if (JsonBase != null)
                        JsonBase["x"] = value;
                });
            }
        }

        private int _y;

        public int Y
        {
            get { return _y; }
            set
            {
                SetProperty(ref _y, value, () =>
                {
                    if (JsonBase != null)
                        JsonBase["y"] = value;
                });
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsWeapon
        {
            get { return ItemGroup == ItemGroup.OneHandWeapon || ItemGroup == ItemGroup.TwoHandWeapon; }
        }

        /// <summary>
        /// vertical range of item
        /// </summary>
        Range<int> IRangeProvider<int>.Range
        {
            get
            {
                return new Range<int>(Y, Y + Height - 1);
            }
        }

        public Item(JObject val)
            : this(ItemClass.Unequipable, val)
        {
            if (Frame < FrameType.Rare)
            {
                _baseType = ItemBase.ItemTypeFromTypeline(TypeLine);
            }

            if (BaseType != null)
            {
                _class = BaseType.Class;
                _itemType = BaseType.ItemType;
                _itemGroup = BaseType.ItemGroup;
            }

            FixOldItems();

        }

        public Item(ItemBase itemBase, int width, int height)
        {
            _baseType = itemBase;
            _class = itemBase.Class;
            _itemType = itemBase.ItemType;
            _itemGroup = itemBase.ItemGroup;
            Width = width;
            Height = height;
        }

        public Item(ItemClass iClass, JObject val)
        {
            JsonBase = val;

            Attributes = new Dictionary<string, List<float>>();
            Mods = new List<ItemMod>();
            _class = iClass;

            Width = val["w"].Value<int>();
            Height = val["h"].Value<int>();
            if (val["x"] != null)
                X = val["x"].Value<int>();
            if (val["y"] != null)
                Y = val["y"].Value<int>();

            if (val["name"] != null)
                NameLine = FilterJsonString(val["name"].Value<string>());

            TypeLine = FilterJsonString(val["typeLine"].Value<string>());
            ItemBase.BaseDictionary.TryGetValue(TypeLine, out _baseType);
            if (BaseType != null)
            {
                _itemType = BaseType.ItemType;
                _itemGroup = BaseType.ItemGroup;
            }

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
                        Properties.Add(ItemMod.CreateMod(this, obj["name"].Value<string>(), Numberfilter));

                        // The name of one property of gems contains the Keywords of that gem.
                        _keywords = new List<string>();
                        string[] sl = obj["name"].Value<string>().Split(',');
                        foreach (string i in sl)
                            _keywords.Add(i.Trim());
                        continue;
                    }

                    foreach (Match m in Numberfilter.Matches(s))
                    {
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                    }
                    string cs = obj["name"].Value<string>() + ": " + (Numberfilter.Replace(s, "#"));

                    var mod = ItemMod.CreateMod(this, obj, Numberfilter2);
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

                var m = ItemMod.CreateMod(this, "Requires " + reqs, Numberfilter);
                m.Value = numbers;
                m.ValueColor = affects;
                _requirements.Add(m);
            }


            if (val["implicitMods"] != null)
                foreach (string s in val["implicitMods"].Values<string>())
                {
                    IEnumerable<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), Numberfilter);
                    Mods.AddRange(mods);

                    _implicitMods.Add(ItemMod.CreateMod(this, s, Numberfilter));
                }
            if (val["explicitMods"] != null)
                foreach (string s in val["explicitMods"].Values<string>())
                {
                    IEnumerable<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), Numberfilter);
                    Mods.AddRange(mods);

                    ExplicitMods.Add(ItemMod.CreateMod(this, s, Numberfilter));
                }

            if (val["craftedMods"] != null)
                foreach (string s in val["craftedMods"].Values<string>())
                {
                    IEnumerable<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), Numberfilter);
                    Mods.AddRange(mods);

                    CraftedMods.Add(ItemMod.CreateMod(this, s, Numberfilter));
                }

            if (val["flavourText"] != null)
                FlavourText = string.Join("\r\n", val["flavourText"].Values<string>());


            if (iClass == ItemClass.Gem)
            {
                switch (val["colour"].Value<string>())
                {
                    case "S":
                        _keywords.Add("Strength");
                        break;

                    case "D":
                        _keywords.Add("Dexterity");
                        break;

                    case "I":
                        _keywords.Add("Intelligence");
                        break;
                }
            }
            else
            {
                _gems = new List<Item>();
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
                    var item = new Item(ItemClass.Gem, obj) { _socketGroup = Sockets[socket++] };
                    _gems.Add(item);
                }
            }
        }

        private void FixOldItems()
        {
            if ((BaseType.Name.EndsWith("Crude Bow") || BaseType.Name.EndsWith("Short Bow") || BaseType.Name.EndsWith("Grove Bow") || BaseType.Name.EndsWith("Thicket Bow")) && Height == 4)
                Height = 3;
            if (ItemGroup == ItemGroup.BodyArmour && Height == 4)
                Height = 3;
        }

        public void SetJsonBase()
        {
            JsonBase = GenerateJson();
        }

        private JObject GenerateJson()
        {
            var j = new JObject(
                new JProperty("w", Width),
                new JProperty("h", Height),
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
                j.Add("flavourText", new JArray(FlavourText));

            return j;
        }

        private static string FilterJsonString(string json)
        {
            return Regex.Replace(json, @"<<[a-zA-Z0-9:]+>>", "");
        }

        // Returns gems linked to specified gem.
        public List<Item> GetLinkedGems(Item gem)
        {
            return Gems.Where(linked => linked != gem && linked._socketGroup == gem._socketGroup).ToList();
        }
    }
}