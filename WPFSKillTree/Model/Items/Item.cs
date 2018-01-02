using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MB.Algodat;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items
{
    public class Item : Notifier, IRangeProvider<int>
    {
        private ItemSlot _slot;
        public ItemSlot Slot
        {
            get { return _slot; }
            set { SetProperty(ref _slot, value); }
        }

        public ItemClass ItemClass { get; }
        public Tags Tags { get; }

        private List<Item> _gems = new List<Item>();
        public IReadOnlyList<Item> Gems
        {
            get { return _gems; }
            set { SetProperty(ref _gems, value.ToList()); }
        }

        public IReadOnlyList<string> Keywords { get; }

        private FrameType _frame;
        public FrameType Frame
        {
            get { return _frame; }
            set { SetProperty(ref _frame, value); }
        }

        private ObservableCollection<ItemMod> _properties = new ObservableCollection<ItemMod>();
        public ObservableCollection<ItemMod> Properties
        {
            get { return _properties; }
            set { SetProperty(ref _properties, value); }
        }

        private readonly ObservableCollection<ItemMod> _requirements = new ObservableCollection<ItemMod>();
        public IReadOnlyList<ItemMod> Requirements
        {
            get { return _requirements; }
        }

        private List<ItemMod> _implicitMods = new List<ItemMod>();
        public List<ItemMod> ImplicitMods
        {
            get { return _implicitMods; }
            set { SetProperty(ref _implicitMods, value); }
        }

        private List<ItemMod> _explicitMods = new List<ItemMod>();
        public List<ItemMod> ExplicitMods
        {
            get { return _explicitMods; }
            set { SetProperty(ref _explicitMods, value); }
        }

        private List<ItemMod> _craftedMods = new List<ItemMod>();
        public List<ItemMod> CraftedMods
        {
            get { return _craftedMods; }
            set { SetProperty(ref _craftedMods, value); }
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
        public int SocketGroup { get; private set; }

        public IItemBase BaseType { get; }

        private readonly string _iconUrl;

        public ItemImage Image { get; }

        private JObject _jsonBase;
        public JObject JsonBase
        {
            get { return _jsonBase; }
            private set { SetProperty(ref _jsonBase, value); }
        }

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

        public int Width { get; }
        public int Height { get; }

        public bool IsWeapon => Tags.HasFlag(Tags.Weapon);

        /// <summary>
        /// vertical range of item
        /// </summary>
        Range<int> IRangeProvider<int>.Range => new Range<int>(Y, Y + Height - 1);

        public Item(IItemBase itemBase)
        {
            BaseType = itemBase;
            ItemClass = itemBase.ItemClass;
            Tags = itemBase.Tags;
            Width = itemBase.InventoryWidth;
            Height = itemBase.InventoryHeight;
            RequirementsFromBase();
            Image = itemBase.Image;
            Properties = new ObservableCollection<ItemMod>(itemBase.GetRawProperties());
        }

        public Item(Item source)
        {
            //_slot, ItemClass, Tags, _gems, _keywords, _frame
            _slot = source._slot;
            ItemClass = source.ItemClass;
            Tags = source.Tags;
            _gems = source._gems.ToList();
            if (source.Keywords != null)
                Keywords = source.Keywords.ToList();
            _frame = source._frame;
            //_properties, _requirements, _explicit-, _implicit-, _craftetMods
            _properties = new ObservableCollection<ItemMod>(source._properties);
            _requirements = new ObservableCollection<ItemMod>(source._requirements);
            _explicitMods = source._explicitMods.ToList();
            _implicitMods = source._implicitMods.ToList();
            _craftedMods = source._craftedMods.ToList();
            //_flavourText, _nameLine, _typeLine, _socketGroup, _baseType, _iconUrl, _image
            _flavourText = source.FlavourText;
            _nameLine = source.NameLine;
            _typeLine = source.TypeLine;
            SocketGroup = source.SocketGroup;
            BaseType = source.BaseType;
            _iconUrl = source._iconUrl;
            Image = source.Image;
            //JsonBase, _x, _y, Width, Height
            JsonBase = new JObject(source.JsonBase);
            _x = source._x;
            _y = source._y;
            Width = source.Width;
            Height = source.Height;
        }

        /// <summary>
        /// Constructor for gems as items. Their properties are only gem tags and what is necessary to get the
        /// correct attributes from ItemDB (level and quality).
        /// </summary>
        public Item(string gemName, IEnumerable<string> tags, int level, int quality, int socketGroup)
        {
            ItemClass = ItemClassEx.ItemClassForGem(gemName);
            Tags = ItemClass.ToTags();
            Keywords = tags.ToList();
            _frame = FrameType.Gem;

            var keywordProp = new ItemMod(string.Join(", ", Keywords), false);
            _properties.Add(keywordProp);
            var levelProp = new ItemMod($"Level: {level}", false, ItemMod.ValueColoring.LocallyAffected);
            _properties.Add(levelProp);
            var qualityProp = new ItemMod($"Quality: +{quality}%", false, ItemMod.ValueColoring.LocallyAffected);
            _properties.Add(qualityProp);

            NameLine = "";
            TypeLine = gemName;
            SocketGroup = socketGroup;

            Width = 1;
            Height = 1;
        }

        public Item(IPersistentData persistentData, JObject val, ItemSlot itemSlot = ItemSlot.Unequipable, bool isGem = false)
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

            JToken iconToken;
            if (val.TryGetValue("icon", out iconToken))
                _iconUrl = iconToken.Value<string>();

            Frame = (FrameType)val["frameType"].Value<int>();
            TypeLine = FilterJsonString(val["typeLine"].Value<string>());
            if (isGem)
            {
                // BaseType will be null for socketed gems.
                ItemClass = ItemClassEx.ItemClassForGem(TypeLine);
                Tags = ItemClass.ToTags();
            }
            else
            {
                if (Frame == FrameType.Magic)
                {
                    BaseType = persistentData.EquipmentData.ItemBaseFromTypeline(TypeLine);
                }
                else if ((Frame == FrameType.Unique || Frame == FrameType.Foil)
                    && persistentData.EquipmentData.UniqueBaseDictionary.ContainsKey(NameLine))
                {
                    BaseType = persistentData.EquipmentData.UniqueBaseDictionary[NameLine];
                }
                else
                {
                    // item is not unique or the unique is unknown
                    ItemBase iBase;
                    persistentData.EquipmentData.ItemBaseDictionary.TryGetValue(TypeLine, out iBase);
                    BaseType = iBase;
                }
                // For known bases, images are only downloaded if the item is unique or foil. All other items should
                // always have the same image. (except alt art non-uniques that are rare enough to be ignored)
                var loadImageFromIconUrl = _iconUrl != null
                    && (BaseType == null || Frame == FrameType.Unique || Frame == FrameType.Foil);
                if (BaseType == null)
                {
                    BaseType = new ItemBase(persistentData.EquipmentData.ItemImageService, itemSlot, TypeLine,
                        Keywords == null ? "" : Keywords.FirstOrDefault(), Frame);
                }
                ItemClass = BaseType.ItemClass;
                Tags = BaseType.Tags;
                if (loadImageFromIconUrl)
                {
                    Image = BaseType.Image.AsDefaultForImageFromUrl(
                        persistentData.EquipmentData.ItemImageService, _iconUrl);
                }
                else
                {
                    Image = BaseType.Image;
                }
            }

            if (val["properties"] != null)
            {
                foreach (var obj in val["properties"])
                {
                    Properties.Add(ItemModFromJson(obj, ModLocation.Property));
                }
                if (Properties.Any(m => !m.Values.Any()))
                {
                    // The name of one property of gems contains the Keywords of that gem.
                    Keywords = Properties.First(m => !m.Values.Any()).Attribute.Split(',').Select(i => i.Trim()).ToList();
                }
            }

            if (val["requirements"] != null)
            {
                var mods = val["requirements"].Select(t => ItemModFromJson(t, ModLocation.Requirement)).ToList();
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
                    mods.Add(new ItemMod(
                        "Requires " + string.Join(", ", modsToMerge.Select(m => m.Attribute)),
                        false,
                        modsToMerge.Select(m => m.Values).Flatten(),
                        modsToMerge.Select(m => m.ValueColors).Flatten()
                    ));
                }
                _requirements.AddRange(mods);
            }


            if (val["implicitMods"] != null)
                foreach (var s in val["implicitMods"].Values<string>())
                {
                    _implicitMods.Add(ItemModFromString(FixOldRanges(s), ModLocation.Implicit));
                }
            if (val["explicitMods"] != null)
                foreach (var s in val["explicitMods"].Values<string>())
                {
                    ExplicitMods.Add(ItemModFromString(FixOldRanges(s), ModLocation.Explicit));
                }
            if (val["craftedMods"] != null)
                foreach (var s in val["craftedMods"].Values<string>())
                {
                    CraftedMods.Add(ItemModFromString(FixOldRanges(s), ModLocation.Crafted));
                }

            if (val["flavourText"] != null && val["flavourText"].HasValues)
                FlavourText = string.Join("\r\n", val["flavourText"].Values<string>().Select(s => s.Replace("\r", "")));

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
                    var item = new Item(persistentData, obj, isGem: true) {SocketGroup = sockets[socket++]};
                    _gems.Add(item);
                }
            }
        }

        private ItemMod ItemModFromString(string attribute, ModLocation location, 
            IEnumerable<ItemMod.ValueColoring> valueColor = null)
        {
            var isLocal = StatLocalityChecker.DetermineLocal(ItemClass, location, attribute);
            var itemMod = new ItemMod(attribute, isLocal);
            if (valueColor != null)
            {
                itemMod.ValueColors = valueColor.ToList();
            }
            return itemMod;
        }

        private ItemMod ItemModFromJson(JToken jsonMod, ModLocation location)
        {
            var valuePairs = (from a in jsonMod["values"]
                              let vc = (ItemMod.ValueColoring)a[1].Value<int>()
                              select new { Value = a[0].Value<string>(), ValueColor = vc }).ToList();
            var values = valuePairs.Select(p => p.Value).ToList();
            var valueColors = (from p in valuePairs
                               let valueCount = ItemMod.Numberfilter.Matches(p.Value).Count
                               select Enumerable.Repeat(p.ValueColor, valueCount))
                               .Flatten();

            /* displayMode:
             * - 0: `attribute = name + ": " + values.Join(", ")` if in "properties" or `attribute = name + " " + value` if in "requirements"
             * - 1: `attribute = values.Join(", ") + " " + name`
             * - 2: experience bar for gems, not applicable
             * - 3: `attribute = name.Replace(%i with values[i])`
             */
            var name = jsonMod["name"].Value<string>();
            var mode0Separator = location == ModLocation.Requirement ? " " : ": ";
            string attribute;
            if (values.Any() && !string.IsNullOrEmpty(name))
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
            else if (string.IsNullOrEmpty(name))
            {
                attribute = string.Join(", ", values);
            }
            else
            {
                attribute = name;
            }

            return ItemModFromString(attribute, location, valueColors);
        }

        private static readonly Regex OldRangeRegex = new Regex(@"(\d+)-(\d+) ");
        private static string FixOldRanges(string range)
        {
            return OldRangeRegex.Replace(range, "$1 to $2 ");
        }

        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Attribute requirements are rounded down")]
        private void RequirementsFromBase(int minRequiredLevel = 0, int attrRequirementsMultiplier = 100)
        {
            var requirements = new List<string>();
            var values = new List<float>();
            var colors = new List<ItemMod.ValueColoring>();
            var attrColor = attrRequirementsMultiplier == 100
                ? ItemMod.ValueColoring.White
                : ItemMod.ValueColoring.LocallyAffected;
            if (BaseType.Level > 1 || minRequiredLevel > 1)
            {
                requirements.Add("Level #");
                values.Add(Math.Max(BaseType.Level, minRequiredLevel));
                colors.Add(ItemMod.ValueColoring.White);
            }
            if (BaseType.RequiredStrength > 0)
            {
                requirements.Add("# Str");
                values.Add((BaseType.RequiredStrength * attrRequirementsMultiplier) / 100);
                colors.Add(attrColor);
            }
            if (BaseType.RequiredDexterity > 0)
            {
                requirements.Add("# Dex");
                values.Add((BaseType.RequiredDexterity * attrRequirementsMultiplier) / 100);
                colors.Add(attrColor);
            }
            if (BaseType.RequiredIntelligence > 0)
            {
                requirements.Add("# Int");
                values.Add((BaseType.RequiredIntelligence * attrRequirementsMultiplier) / 100);
                colors.Add(attrColor);
            }
            if (requirements.Any())
            {
                _requirements.Add(new ItemMod("Requires " + string.Join(", ", requirements), true, values, colors));
            }
        }

        public void UpdateRequirements(int minRequiredLevel)
        {
            _requirements.Clear();
            var attrRequirementsMultiplier = 100 - Mods
                .Where(m => m.Attribute == "#% reduced Attribute Requirements")
                .Where(m => m.Values.Any())
                .Select(m => (int)m.Values[0])
                .DefaultIfEmpty(0)
                .Sum();
            RequirementsFromBase(minRequiredLevel, attrRequirementsMultiplier);
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
            if (_iconUrl != null)
                j["icon"] = _iconUrl;

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

            if (Gems.Count > 0)
            {
                var sockets = new JArray();
                var socketedItems = new JArray();
                foreach (var gem in Gems)
                {
                    sockets.Add(new JObject { { "group", gem.SocketGroup } });
                    socketedItems.Add(gem.JsonBase);
                }
                j["sockets"] = sockets;
                j["socketedItems"] = socketedItems;
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
            return Gems.Where(linked => linked != gem && linked.SocketGroup == gem.SocketGroup).ToList();
        }

        /// <summary>
        /// Key: Property
        /// Value: Mods affecting the Property
        /// </summary>
        public Dictionary<ItemMod, List<ItemMod>> GetModsAffectingProperties()
        {
            var qualityMod = Properties.FirstOrDefault(m => m.Attribute == "Quality: +#%");
            // Quality with "+#%" in name is not recognized as percentage increase.
            var qIncMod = qualityMod == null ? null
                : new ItemMod(qualityMod.Values[0] + "%", true);
            var localmods = Mods.Where(m => m.IsLocal).ToList();

            var r = new Regex(@"(?<=[^a-zA-Z] |^)(to|increased|decreased|more|less) |^Adds # to # |(\+|-|#|%|:|\s\s)\s*?(?=\s?)|^\s+|\s+$");

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
                dict[mod] = localmods.Where((m, i) =>
                        localnames[i].Any(n => mod.Attribute.Contains(n, StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
                // Add quality if this property is affected by quality
                var name = mod.Attribute;
                if (qualityMod != null &&
                    (name.Contains("physical damage", StringComparison.InvariantCultureIgnoreCase)
                    || name.Contains("evasion rating", StringComparison.InvariantCultureIgnoreCase)
                    || name.Contains("armour", StringComparison.InvariantCultureIgnoreCase)
                    || name.Contains("energy shield", StringComparison.InvariantCultureIgnoreCase)))
                {
                    dict[mod].Add(qIncMod);
                }
            }
            return dict;
        }
    }
}