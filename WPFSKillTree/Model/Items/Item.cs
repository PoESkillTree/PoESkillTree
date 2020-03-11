using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EnumsNET;
using MB.Algodat;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Modifiers;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Items.Mods;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.Items
{
    public class Item : Notifier, IRangeProvider<int>, IHasItemToolTip
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private ItemSlot _slot;
        public ItemSlot Slot
        {
            get => _slot;
            set => SetProperty(ref _slot, value);
        }

        private ushort? _socket;
        public ushort? Socket
        {
            get => _socket;
            set => SetProperty(ref _socket, value);
        }

        public ItemClass ItemClass { get; }
        public Tags Tags { get; }
        public bool IsFlask => Tags.HasFlag(Tags.Flask);
        public bool IsJewel => ItemClass == ItemClass.Jewel || ItemClass == ItemClass.AbyssJewel;

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private FrameType _frame;
        public FrameType Frame
        {
            get => _frame;
            set => SetProperty(ref _frame, value);
        }

        private ObservableCollection<ItemMod> _observableProperties = new ObservableCollection<ItemMod>();
        public IReadOnlyList<ItemMod> Properties => _observableProperties;

        private ObservableCollection<ItemMod> ObservableProperties
        {
            get => _observableProperties;
            set => SetProperty(ref _observableProperties, value, () => OnPropertyChanged(nameof(Properties)));
        }

        public JewelRadius JewelRadius
        {
            get
            {
                if (ItemClass != ItemClass.Jewel)
                    return JewelRadius.None;

                var radiusString = Properties
                    .FirstOrDefault(m => m.Attribute.StartsWith("Radius: "))
                    ?.Attribute.Substring("Radius: ".Length);
                if (radiusString != null && Enums.TryParse(radiusString, out JewelRadius radius))
                    return radius;
                return JewelRadius.None;
            }
        }

        private readonly ObservableCollection<ItemMod> _requirements = new ObservableCollection<ItemMod>();
        public IReadOnlyList<ItemMod> Requirements => _requirements;

        private IReadOnlyList<ItemMod> _implicitMods = new List<ItemMod>();
        public IReadOnlyList<ItemMod> ImplicitMods
        {
            get => _implicitMods;
            set => SetProperty(ref _implicitMods, value);
        }

        private IReadOnlyList<ItemMod> _explicitMods = new List<ItemMod>();
        public IReadOnlyList<ItemMod> ExplicitMods
        {
            get => _explicitMods;
            set => SetProperty(ref _explicitMods, value);
        }

        private IReadOnlyList<ItemMod> _craftedMods = new List<ItemMod>();
        public IReadOnlyList<ItemMod> CraftedMods
        {
            get => _craftedMods;
            set => SetProperty(ref _craftedMods, value);
        }

        private string? _flavourText;
        public string? FlavourText
        {
            get => _flavourText;
            set => SetProperty(ref _flavourText, value, () => OnPropertyChanged(nameof(HasFlavourText)));
        }

        public bool HasFlavourText =>
            !string.IsNullOrEmpty(FlavourText);

        public IEnumerable<ItemMod> Mods
            => ImplicitMods.Union(ExplicitMods).Union(CraftedMods);

        public string Name
            => string.IsNullOrEmpty(NameLine) ? TypeLine : NameLine;

        private string _nameLine = "";
        public string NameLine
        {
            get => _nameLine;
            set => SetProperty(ref _nameLine, value, () => OnPropertyChanged(nameof(IHasItemToolTip.HasNameLine)));
        }

        public bool HasNameLine =>
            !string.IsNullOrEmpty(NameLine);

        private string _typeLine = "";
        public string TypeLine
        {
            get => _typeLine;
            set => SetProperty(ref _typeLine, value);
        }

        public IItemBase BaseType { get; }

        private readonly string? _iconUrl;

        public ItemImage Image { get; }

        private int _x;
        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        private int _y;
        public int Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
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
            ObservableProperties = new ObservableCollection<ItemMod>(itemBase.GetRawProperties());
        }

        public Item(Item source)
        {
            _slot = source._slot;
            _socket = source._socket;
            ItemClass = source.ItemClass;
            Tags = source.Tags;
            _frame = source._frame;
            _isEnabled = source._isEnabled;

            _observableProperties = new ObservableCollection<ItemMod>(source._observableProperties);
            _requirements = new ObservableCollection<ItemMod>(source._requirements);
            _explicitMods = source._explicitMods.ToList();
            _implicitMods = source._implicitMods.ToList();
            _craftedMods = source._craftedMods.ToList();

            _flavourText = source.FlavourText;
            _nameLine = source.NameLine;
            _typeLine = source.TypeLine;
            BaseType = source.BaseType;
            _iconUrl = source._iconUrl;
            Image = source.Image;

            _x = source._x;
            _y = source._y;
            Width = source.Width;
            Height = source.Height;
        }

        public Item(EquipmentData equipmentData, JObject val, ItemSlot itemSlot = ItemSlot.Unequipable)
        {
            Slot = itemSlot;
            Socket = val["socket"]?.Value<ushort?>();

            Width = val["w"]?.Value<int?>() ?? 1;
            Height = val["h"]?.Value<int?>() ?? 1;
            X = val["x"]?.Value<int?>() ?? 0;
            Y = val["y"]?.Value<int?>() ?? 0;

            if (val.TryGetValue("name", out var nameToken))
                NameLine = FilterJsonString(nameToken.Value<string>());

            if (val.TryGetValue("icon", out var iconToken))
                _iconUrl = iconToken.Value<string>();

            if (val.TryGetValue("isEnabled", out var enabledToken))
                IsEnabled = enabledToken.Value<bool>();

            Frame = (FrameType) val["frameType"]!.Value<int>();
            TypeLine = FilterJsonString(val["typeLine"]!.Value<string>());

            IItemBase? baseType = null;
            if (Frame == FrameType.Magic)
            {
                baseType = equipmentData.ItemBaseFromTypeline(TypeLine);
            }
            else if ((Frame == FrameType.Unique || Frame == FrameType.Foil)
                     && equipmentData.UniqueBaseDictionary.TryGetValue(NameLine, out var uBase))
            {
                baseType = uBase;
            }
            else if (equipmentData.ItemBaseDictionary.TryGetValue(TypeLine, out var iBase))
            {
                // item is not unique or the unique is unknown
                baseType = iBase;
            }
            // For known bases, images are only downloaded if the item is unique or foil. All other items should
            // always have the same image. (except alt art non-uniques that are rare enough to be ignored)
            var loadImageFromIconUrl = baseType == null || Frame == FrameType.Unique || Frame == FrameType.Foil;
            BaseType = baseType ?? new ItemBase(equipmentData.ItemImageService, itemSlot, TypeLine, Frame);
            ItemClass = BaseType.ItemClass;
            Tags = BaseType.Tags;
            if (_iconUrl != null && loadImageFromIconUrl)
            {
                Image = BaseType.Image.AsDefaultForImageFromUrl(
                    equipmentData.ItemImageService, _iconUrl);
            }
            else
            {
                Image = BaseType.Image;
            }

            if (val.TryGetValue("properties", out var propertiesToken))
            {
                foreach (var obj in propertiesToken)
                {
                    ObservableProperties.Add(ItemModFromJson(obj, false));
                }
            }

            if (val.TryGetValue("requirements", out var requirementsToken))
            {
                var mods = requirementsToken.Select(t => ItemModFromJson(t, true)).ToList();
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

            if (val.TryGetValue("implicitMods", out var implicitModsToken))
            {
                ImplicitMods = implicitModsToken.Values<string>().Select(s => ItemModFromString(s)).ToList();
            }

            if (val.TryGetValue("fracturedMods", out var fracturedModsToken))
            {
                ExplicitMods = fracturedModsToken.Values<string>().Select(s => ItemModFromString(s)).ToList();
            }

            if (val.TryGetValue("explicitMods", out var explicitModsToken))
            {
                var additionalExplicitMods = explicitModsToken.Values<string>().Select(s => ItemModFromString(s));
                ExplicitMods = ExplicitMods.Concat(additionalExplicitMods).ToList();
            }

            if (val.TryGetValue("craftedMods", out var craftedModsToken))
            {
                CraftedMods = craftedModsToken.Values<string>().Select(s => ItemModFromString(s)).ToList();
            }

            if (val.TryGetValue("flavourText", out var flavourTextToken) && flavourTextToken.HasValues)
                FlavourText = string.Join("\r\n", flavourTextToken.Values<string>().Select(s => s.Replace("\r", "")));
        }

        public IEnumerable<Item> DeserializeSocketedItems(EquipmentData equipmentData, JObject itemJson)
        {
            if (!itemJson.TryGetValue("socketedItems", out var socketedItemsJson))
                yield break;

            ushort socket = 0;
            foreach (var socketedItemJson in socketedItemsJson.Values<JObject>())
            {
                var frameType = socketedItemJson.Value<int>("frameType");
                if ((FrameType) frameType != FrameType.Gem)
                {
                    yield return new Item(equipmentData, socketedItemJson, Slot) {Socket = socket};
                    socket++;
                }
            }
        }

        public IReadOnlyList<Gem> DeserializeSocketedGems(SkillDefinitions skillDefinitions, JObject itemJson)
        {
            if (!itemJson.TryGetValue("socketedItems", out var skillJson))
                return Array.Empty<Gem>();

            var sockets = new List<int>();
            if (itemJson.TryGetValue("sockets", out var socketsJson))
            {
                foreach (var obj in (JArray) socketsJson)
                {
                    sockets.Add(obj["group"]!.Value<int>());
                }
            }

            var gems = new List<Gem>();
            foreach (var obj in skillJson.Values<JObject>())
            {
                var frameType = obj.Value<int>("frameType");
                if ((FrameType) frameType == FrameType.Gem)
                {
                    if (TryDeserializeSocketedGem(skillDefinitions, obj, sockets, out var gem))
                    {
                        gems.Add(gem);
                    }
                }
            }
            return gems;
        }

        private bool TryDeserializeSocketedGem(
            SkillDefinitions skillDefinitions, JObject jObject, IReadOnlyList<int> socketGroups, [NotNullWhen(true)] out Gem? gem)
        {
            var baseItemSkills = skillDefinitions.Skills
                .Where(d => d.BaseItem != null)
                .Where(d => d.BaseItem!.ReleaseState == ReleaseState.Released
                            || d.BaseItem.ReleaseState == ReleaseState.Legacy)
                .ToList();
            var socketIndex = jObject.Value<int>("socket");
            var name = jObject.Value<string>("typeLine");
            var definition = baseItemSkills.FirstOrDefault(d => d.BaseItem?.DisplayName == name)
                             ?? baseItemSkills.FirstOrDefault(d => d.BaseItem?.DisplayName == name + " Support");
            if (definition is null)
            {
                Log.Error($"Unknown skill: {name}");
                gem = null;
                return false;
            }

            var properties = jObject["properties"].Select(j => ItemModFromJson(j, false)).ToList();
            if (!properties.TryGetValue("Level: #", 0, out var level))
            {
                level = (int) properties.First("Level: # (Max)", 0, 1);
            }
            var quality = (int) properties.First("Quality: +#%", 0, 0);
            gem = new Gem(definition.Id, (int) level, quality, Slot, socketIndex, socketGroups[socketIndex], true);
            return true;
        }

        private ItemMod ItemModFromString(string attribute, IEnumerable<ValueColoring>? valueColor = null)
        {
            var isLocal = ModifierLocalityTester.IsLocal(attribute, Tags);
            var itemMod = new ItemMod(attribute, isLocal);
            if (valueColor != null)
            {
                itemMod.ValueColors = valueColor.ToList();
            }
            return itemMod;
        }

        private ItemMod ItemModFromJson(JToken jsonMod, bool isRequirement)
        {
            var valuePairs = (from a in jsonMod["values"]
                              let vc = (ValueColoring)a[1]!.Value<int>()
                              select new { Value = a[0]!.Value<string>(), ValueColor = vc }).ToList();
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
            var name = jsonMod["name"]!.Value<string>();
            var mode0Separator = isRequirement ? " " : ": ";
            string attribute;
            if (values.Any() && !string.IsNullOrEmpty(name))
            {
                switch (int.Parse(jsonMod["displayMode"]!.Value<string>(), CultureInfo.InvariantCulture))
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
                                                        jsonMod["displayMode"]!.Value<string>());
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

            return ItemModFromString(attribute, valueColors);
        }

        [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Attribute requirements are rounded down")]
        private void RequirementsFromBase(int minRequiredLevel = 0, int attrRequirementsMultiplier = 100)
        {
            var requirements = new List<string>();
            var values = new List<float>();
            var colors = new List<ValueColoring>();
            var attrColor = attrRequirementsMultiplier == 100
                ? ValueColoring.White
                : ValueColoring.LocallyAffected;
            if (BaseType.Level > 1 || minRequiredLevel > 1)
            {
                requirements.Add("Level #");
                values.Add(Math.Max(BaseType.Level, minRequiredLevel));
                colors.Add(ValueColoring.White);
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

        public JObject GenerateJson()
        {
            var j = new JObject(
                new JProperty("w", Width),
                new JProperty("h", Height),
                new JProperty("x", X),
                new JProperty("y", Y),
                new JProperty("name", NameLine),
                new JProperty("typeLine", TypeLine),
                new JProperty("frameType", Frame),
                new JProperty("isEnabled", IsEnabled)
                );
            if (_iconUrl != null)
                j["icon"] = _iconUrl;
            if (_socket.HasValue)
                j["socket"] = _socket;

            if (Properties.Count > 0)
            {
                j.Add(new JProperty("properties",
                    new JArray(Properties.Select(p => p.ToJObject()).ToArray())));
            }

            if (Requirements.Count > 0)
            {
                j.Add(new JProperty("requirements",
                        new JArray(Requirements.Select(p => p.ToJObject()).ToArray())));
            }

            if (ImplicitMods.Count > 0)
            {
                j.Add(new JProperty("implicitMods",
                            new JArray(ImplicitMods.Select(p => p.ToJObject(true)).ToArray())));
            }

            if (ExplicitMods.Count > 0)
            {
                j.Add(new JProperty("explicitMods",
                            new JArray(ExplicitMods.Select(p => p.ToJObject(true)).ToArray())));
            }

            if (CraftedMods.Count > 0)
            {
                j.Add(new JProperty("craftedMods",
                            new JArray(CraftedMods.Select(p => p.ToJObject(true)).ToArray())));
            }

            if (HasFlavourText)
                j.Add("flavourText", new JArray(FlavourText!));

            return j;
        }

        private static string FilterJsonString(string json)
        {
            return Regex.Replace(json, @"<<[a-zA-Z0-9:]+>>", "");
        }

        public void UpdateProperties(int quality, params ItemMod[] additionalProperties)
        {
            var baseProperties = BaseType.GetRawProperties(quality);
            ObservableProperties = new ObservableCollection<ItemMod>(baseProperties.Concat(additionalProperties));
            ApplyLocalModsToProperties();
            if (IsWeapon)
            {
                AddElementalDamageProperties();
            }
        }

        private void ApplyLocalModsToProperties()
        {
            foreach (var pair in GetModsAffectingProperties())
            {
                ItemMod prop = pair.Key;
                List<ItemMod> applymods = pair.Value;

                List<ItemMod> percm = applymods.Where(m => Regex.IsMatch(m.Attribute, @"(?<!\+)#%")).ToList();
                List<ItemMod> valuem = applymods.Except(percm).ToList();

                if (valuem.Count > 0)
                {
                    IReadOnlyList<float> val = valuem
                        .Select(m => m.Values)
                        .Aggregate((l1, l2) => l1.Zip(l2, (f1, f2) => f1 + f2)
                            .ToList());
                    IReadOnlyList<float> nval = prop.Values
                        .Zip(val, (f1, f2) => f1 + f2)
                        .ToList();
                    prop.ValueColors = prop.ValueColors
                        .Select((c, i) => val[i] == nval[i] ? prop.ValueColors[i] : ValueColoring.LocallyAffected)
                        .ToList();
                    prop.Values = nval;
                }

                if (percm.Count > 0)
                {
                    Func<float, float> roundf = val => (float)Math.Round(val);
                    if (prop.Attribute.Contains("Critical"))
                    {
                        roundf = f => (float)(Math.Round(f * 10) / 10);
                    }
                    else if (prop.Attribute.Contains("per Second"))
                    {
                        roundf = f => (float)(Math.Round(f * 100) / 100);
                    }

                    var perc = 1f + percm.Select(m => m.Values[0]).Sum() / 100f;
                    prop.ValueColors = prop.ValueColors.Select(_ => ValueColoring.LocallyAffected).ToList();
                    prop.Values = prop.Values.Select(v => roundf(v * perc)).ToList();
                }
            }
        }

        private void AddElementalDamageProperties()
        {
            var elementalMods = new List<ItemMod>();
            var chaosMods = new List<ItemMod>();
            foreach (var mod in Mods)
            {
                string attr = mod.Attribute;
                if (attr.StartsWith("Adds") && !attr.Contains("in Main Hand") && !attr.Contains("in Off Hand"))
                {
                    if (attr.Contains("Fire") || attr.Contains("Cold") || attr.Contains("Lightning"))
                    {
                        elementalMods.Add(mod);
                    }
                    if (attr.Contains("Chaos"))
                    {
                        chaosMods.Add(mod);
                    }
                }
            }

            if (elementalMods.Any())
            {
                var values = new List<float>();
                var mods = new List<string>();
                var cols = new List<ValueColoring>();

                var fmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Fire"));
                if (fmod != null)
                {
                    values.AddRange(fmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Fire);
                    cols.Add(ValueColoring.Fire);
                }

                var cmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                if (cmod != null)
                {
                    values.AddRange(cmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Cold);
                    cols.Add(ValueColoring.Cold);
                }

                var lmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
                if (lmod != null)
                {
                    values.AddRange(lmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Lightning);
                    cols.Add(ValueColoring.Lightning);
                }

                ObservableProperties.Add(new ItemMod("Elemental Damage: " + string.Join(", ", mods), true, values, cols));
            }

            if (chaosMods.Any())
            {
                ObservableProperties.Add(new ItemMod("Chaos Damage: #-#", true, chaosMods[0].Values,
                    new[] { ValueColoring.Chaos, ValueColoring.Chaos }));
            }
        }

        /// <summary>
        /// Key: Property
        /// Value: Mods affecting the Property
        /// </summary>
        private Dictionary<ItemMod, List<ItemMod>> GetModsAffectingProperties()
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
                    dict[mod].Add(qIncMod!);
                }
            }
            return dict;
        }
    }
}