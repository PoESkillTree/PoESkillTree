using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves item bases from RePoE and converts them into a XML format.
    /// </summary>
    public class ItemBaseLoader : XmlDataLoader<XmlItemList>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemBaseLoader));

        private const string RepoeUrl = DataUtils.RePoEDataUrl + "base_items.min.json";

        private static readonly ISet<ItemClass> ItemClassWhitelist = new HashSet<ItemClass>
        {
            ItemClass.OneHandSword,
            ItemClass.ThrustingOneHandSword,
            ItemClass.OneHandAxe,
            ItemClass.OneHandMace,
            ItemClass.Sceptre,
            ItemClass.Dagger,
            ItemClass.Claw,
            ItemClass.Wand,

            ItemClass.TwoHandSword,
            ItemClass.TwoHandAxe,
            ItemClass.TwoHandMace,
            ItemClass.Bow,
            ItemClass.Staff,
            ItemClass.FishingRod,

            ItemClass.Belt,
            ItemClass.Ring,
            ItemClass.Amulet,
            ItemClass.Quiver,

            ItemClass.Shield,
            ItemClass.Boots,
            ItemClass.BodyArmour,
            ItemClass.Gloves,
            ItemClass.Helmet,

            ItemClass.Jewel
        };

        private readonly HashSet<string> _unknownTags = new HashSet<string>();

        protected override async Task LoadAsync()
        {
            Task<string> jsonTask = HttpClient.GetStringAsync(RepoeUrl);
            var json = JObject.Parse(await jsonTask);
            Data = new XmlItemList
            {
                ItemBases = CreateXmlItemBases(json).ToArray()
            };
            Log.Info("Unknown tags: " + string.Join(", ", _unknownTags));
        }

        private IEnumerable<XmlItemBase> CreateXmlItemBases(JObject json)
        {
            return
                from property in json.Properties()
                let itemBaseJson = property.Value
                where HasValidItemClass(itemBaseJson)
                      && HasValidReleaseState(itemBaseJson)
                let metadataId = property.Name
                let itemBase = CreateXmlItemBaseFromJson(metadataId, itemBaseJson)
                orderby itemBase.MetadataId
                select itemBase;
        }

        private static bool HasValidItemClass(JToken itemBaseJson)
        {
            return ItemClassEx.TryParse(itemBaseJson.Value<string>("item_class"), out var itemClass)
                   && ItemClassWhitelist.Contains(itemClass);
        }

        private static bool HasValidReleaseState(JToken itemBaseJson)
        {
            return itemBaseJson.Value<string>("release_state") != "unreleased";
        }

        private XmlItemBase CreateXmlItemBaseFromJson(string metadataId, JToken obj)
        {
            var converter = new ItemBaseJsonToXmlConverter(metadataId, obj);
            var itemBase = converter.Parse();
            _unknownTags.UnionWith(converter.UnknownTags);
            return itemBase;
        }
    }

    internal class ItemBaseJsonToXmlConverter
    {
        private readonly string _metadataId;
        private readonly JToken _json;

        private XmlItemBase _xml;

        public IReadOnlyCollection<string> UnknownTags { get; private set; }

        public ItemBaseJsonToXmlConverter(string metadataId, JToken json)
        {
            _metadataId = metadataId;
            _json = json;
        }

        public XmlItemBase Parse()
        {
            _xml = new XmlItemBase();
            ParseSimpleFields();
            ParseItemClass();
            ParseImplicits();
            ParseRequirements();
            ParseTags();
            ParseProperties();
            return _xml;
        }

        private void ParseSimpleFields()
        {
            _xml.Name = _json.Value<string>("name");
            _xml.DropDisabled = _json.Value<string>("release_state") != "released";
            _xml.InventoryHeight = _json.Value<int>("inventory_height");
            _xml.InventoryWidth = _json.Value<int>("inventory_width");
            _xml.MetadataId = _metadataId;
        }

        private void ParseItemClass()
        {
            ItemClassEx.TryParse(_json.Value<string>("item_class"), out var itemClass);
            _xml.ItemClass = itemClass;
        }

        private void ParseImplicits()
        {
            _xml.Implicit = _json["implicits"].Values<string>().ToArray();
        }

        private void ParseRequirements()
        {
            var requirements = _json["requirements"];
            if (requirements.HasValues)
            {
                _xml.Dexterity = requirements.Value<int>("dexterity");
                _xml.Strength = requirements.Value<int>("strength");
                _xml.Intelligence = requirements.Value<int>("intelligence");
                _xml.Level = requirements.Value<int>("level");
            }
            else
            {
                _xml.Level = 1;
            }
        }

        private void ParseTags()
        {
            var unknownTags = new HashSet<string>();
            foreach (var s in _json["tags"].Values<string>())
            {
                if (TagsEx.TryParse(s, out var tag))
                {
                    _xml.Tags |= tag;
                }
                else
                {
                    unknownTags.Add(s);
                }
            }

            UnknownTags = unknownTags.ToList();
        }

        private void ParseProperties()
        {
            var properties = _json["properties"];
            if (_xml.Tags.HasFlag(Tags.Weapon))
            {
                _xml.Properties = FormatToArray(ParseWeaponProperties(properties));
            }
            else if (_xml.Tags.HasFlag(Tags.Armour))
            {
                _xml.Properties = FormatToArray(ParseArmourProperties(properties));
            }
            else
            {
                _xml.Properties = new string[0];
            }
        }

        private static string[] FormatToArray(IEnumerable<FormattableString> properties)
        {
            return properties.Select(FormattableString.Invariant).ToArray();
        }

        private static IEnumerable<FormattableString> ParseWeaponProperties(JToken properties)
        {
            yield return
                $"Physical Damage: {properties.Value<string>("physical_damage_min")}-{properties.Value<string>("physical_damage_max")}";
            yield return
                $"Critical Strike Chance: {properties.Value<int>("critical_strike_chance") / 100.0:##.##}%";
            yield return
                $"Attacks per Second: {1000.0 / properties.Value<int>("attack_time"):##.##}";
            yield return
                $"Weapon Range: {properties.Value<int>("range")}";
        }

        private static IEnumerable<FormattableString> ParseArmourProperties(JToken properties)
        {
            if (properties["block"]?.Value<int>() is int block)
            {
                yield return $"Chance to Block: {block}%";
            }

            if (properties["armour"]?.Value<int>() is int armour)
            {
                yield return $"Armour: {armour}";
            }

            if (properties["evasion"]?.Value<int>() is int evasion)
            {
                yield return $"Evasion Rating: {evasion}";
            }

            if (properties["energy_shield"]?.Value<int>() is int energyShield)
            {
                yield return $"Energy Shield: {energyShield}";
            }
        }
    }
}