using System;
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

        public IEnumerable<ItemMod> Mods
        {
            get { return ImplicitMods.Union(ExplicitMods).Union(CraftedMods); }
        }

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
            get { return ItemGroup == ItemGroup.OneHandedWeapon || ItemGroup == ItemGroup.TwoHandedWeapon; }
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

        public Item(ItemBase itemBase, int width, int height)
        {
            _baseType = itemBase;
            _itemType = itemBase.ItemType;
            _itemGroup = itemBase.ItemGroup;
            Width = width;
            Height = height;
            RequirementsFromBase();
        }

        public Item(JObject val, ItemSlot itemSlot = ItemSlot.Unequipable, bool isGem = false)
        {
            JsonBase = val;
            Slot = itemSlot;

            Width = val["w"].Value<int>();
            Height = val["h"].Value<int>();
            if (val["x"] != null)
                X = val["x"].Value<int>();
            if (val["y"] != null)
                Y = val["y"].Value<int>();

            if (val["name"] != null)
                NameLine = FilterJsonString(val["name"].Value<string>());

            if (val["properties"] != null)
            {
                foreach (var obj in val["properties"])
                {
                    Properties.Add(ItemModFromJson(obj, false));
                }
                if (Properties.Any(m => !m.Value.Any()))
                {
                    // The name of one property of gems contains the Keywords of that gem.
                    _keywords = Properties.First(m => !m.Value.Any()).Attribute.Split(',').Select(i => i.Trim()).ToList();
                }
            }

            if (val["requirements"] != null)
            {
                var mods = val["requirements"].Select(t => ItemModFromJson(t, true)).ToList();
                if (!mods.Any(m => m.Attribute.StartsWith("Requires ")))
                {
                    var modsToMerge = new []
                    {
                        mods.FirstOrDefault(m => m.Attribute == "Level #"),
                        mods.FirstOrDefault(m => m.Attribute == "# Str"),
                        mods.FirstOrDefault(m => m.Attribute == "# Dex"),
                        mods.FirstOrDefault(m => m.Attribute == "# Int")
                    }.Where(m => m != null).ToList();
                    modsToMerge.ForEach(m => mods.Remove(m));
                    mods.Add(new ItemMod(_itemType, "Requires " + string.Join(", ", modsToMerge.Select(m => m.Attribute)))
                    {
                        Value = modsToMerge.Select(m => m.Value).Flatten().ToList(),
                        ValueColor = modsToMerge.Select(m => m.ValueColor).Flatten().ToList()
                    });
                }
                _requirements.AddRange(mods);
            }


            if (val["implicitMods"] != null)
                foreach (var s in val["implicitMods"].Values<string>())
                {
                    _implicitMods.Add(new ItemMod(_itemType, s, Numberfilter));
                }
            if (val["explicitMods"] != null)
                foreach (var s in val["explicitMods"].Values<string>())
                {
                    ExplicitMods.Add(new ItemMod(_itemType, s, Numberfilter));
                }
            if (val["craftedMods"] != null)
                foreach (var s in val["craftedMods"].Values<string>())
                {
                    CraftedMods.Add(new ItemMod(_itemType, s, Numberfilter));
                }

            if (val["flavourText"] != null)
                FlavourText = string.Join("\r\n", val["flavourText"].Values<string>().Select(s => s.Replace("\r", "")));


            if (isGem)
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

            var sockets = new List<int>();
            if (val["sockets"] != null)
                foreach (var obj in (JArray)val["sockets"])
                {
                    sockets.Add(obj["group"].Value<int>());
                }
            if (val["socketedItems"] != null)
            {
                int socket = 0;
                foreach (JObject obj in (JArray)val["socketedItems"])
                {
                    var item = new Item(obj, isGem: true) { _socketGroup = sockets[socket++] };
                    _gems.Add(item);
                }
            }

            Frame = (FrameType)val["frameType"].Value<int>();
            TypeLine = FilterJsonString(val["typeLine"].Value<string>());
            if (isGem)
            {
                // BaseType will be null for gems.
                _itemGroup = ItemGroup.Gem;
            }
            else
            {
                if (Frame < FrameType.Rare)
                {
                    _baseType = ItemBase.ItemBaseFromTypeline(TypeLine);
                }
                else
                {
                    ItemBase.BaseDictionary.TryGetValue(TypeLine, out _baseType);
                }
                if (_baseType == null)
                {
                    _baseType = new ItemBase(itemSlot, TypeLine, _keywords == null ? "" : _keywords.FirstOrDefault());
                }
                _itemType = BaseType.ItemType;
                _itemGroup = BaseType.ItemGroup;

                FixOldItems();
            }
        }

        private ItemMod ItemModFromJson(JToken jsonMod, bool areRequirements)
        {
            var valuePairs = (from a in jsonMod["values"]
                              let vc = (ItemMod.ValueColoring)a[1].Value<int>()
                              select new { Value = a[0].Value<string>(), ValueColor = vc }).ToList();
            var values = valuePairs.Select(p => p.Value).ToList();
            var valueColors = (from p in valuePairs
                               let valueCount = Numberfilter.Matches(p.Value).Count
                               select Enumerable.Repeat(p.ValueColor, valueCount))
                               .Flatten();

            /* displayMode:
             * - 0: `attribute = name + ": " + values.Join(", ")` if in "properties" or `attribute = name + " " + value` if in "requirements"
             * - 1: `attribute = values.Join(", ") + " " + name`
             * - 2: experience bar for gems, not applicable
             * - 3: `attribute = name.Replace(%i with values[i])`
             */
            var name = jsonMod["name"].Value<string>();
            var mode0Separator = areRequirements ? " " : ": ";
            string attribute;
            if (values.Any())
            {
                switch (int.Parse(jsonMod["displayMode"].Value<string>(), CultureInfo.InvariantCulture))
                {
                    case 0:
                        attribute = name + mode0Separator + string.Join(", ", values);
                        break;
                    case 1:
                        attribute = string.Join(", ", values) + " " + name;
                        break;
                    case 2:
                        throw new NotSupportedException("Experience bar display mode is not supported.");
                    case 3:
                        attribute = Regex.Replace(name, @"%\d", m => values[int.Parse(m.Value.Substring(1))]);
                        break;
                    default:
                        throw new NotSupportedException("Unsupported display mode: " +
                                                        jsonMod["displayMode"].Value<string>());
                }
            }
            else
            {
                attribute = name;
            }

            return new ItemMod(_itemType, attribute, Numberfilter, valueColors);
        }

        private void FixOldItems()
        {
            if ((BaseType.Name.EndsWith("Crude Bow") || BaseType.Name.EndsWith("Short Bow") || BaseType.Name.EndsWith("Grove Bow") || BaseType.Name.EndsWith("Thicket Bow")) && Height == 4)
                Height = 3;
            if (ItemGroup == ItemGroup.BodyArmour && Height == 4)
                Height = 3;
        }

        private void RequirementsFromBase()
        {
            var requirements = new List<string>();
            var values = new List<float>();
            if (BaseType.Level > 0)
            {
                requirements.Add("Level #");
                values.Add(BaseType.Level);
            }
            if (BaseType.RequiredStrength > 0)
            {
                requirements.Add("# Str");
                values.Add(BaseType.RequiredStrength);
            }
            if (BaseType.RequiredDexterity > 0)
            {
                requirements.Add("# Dex");
                values.Add(BaseType.RequiredDexterity);
            }
            if (BaseType.RequiredIntelligence > 0)
            {
                requirements.Add("# Int");
                values.Add(BaseType.RequiredIntelligence);
            }
            if (requirements.Any())
            {
                _requirements.Add(new ItemMod(_itemType, "Requires " + string.Join(", ", requirements))
                {
                    Value = values,
                    ValueColor = Enumerable.Repeat(ItemMod.ValueColoring.White, values.Count).ToList()
                });
            }
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

        /// <summary>
        /// Key: Property
        /// Value: Mods affecting the Property
        /// </summary>
        public Dictionary<ItemMod, List<ItemMod>> GetModsAffectingProperties()
        {
            var localmods = Mods.Where(m => m.IsLocal).ToList();

            var r = new Regex(@"(?<=[^a-zA-Z] |^)(to|increased|decreased|more|less) |^Adds #-# |(\+|-|#|%|:|\s\s)\s*?(?=\s?)|^\s+|\s+$");

            var localnames = localmods.Select(m =>
                r.Replace(m.Attribute.Replace("to maximum", "to"), "")
                    .Split(new[] { "and", "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s =>
                        s.Trim().Replace("Attack Speed", "Attacks per Second"))
                        .ToList())
                .ToList();

            var dict = new Dictionary<ItemMod, List<ItemMod>>();
            foreach (var mod in Properties)
            {
                dict[mod] = localmods.Where((m, i) => localnames[i].Any(n => mod.Attribute.Contains(n))).ToList();
            }
            return dict;
        }
    }
}