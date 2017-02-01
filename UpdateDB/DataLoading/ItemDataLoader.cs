using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.WikiApi;

using static POESKillTree.Utils.WikiApi.WikiApiUtils;
using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves item bases from the Wiki through its API.
    /// </summary>
    public class ItemDataLoader : XmlDataLoader<XmlItemList>
    {
        /* Conditions:
         * - Has Rarity::Normal
         * - Is drop enabled::true
         * - Has item class::{itemType}
         * Printout global:
         * - Has name
         * - Has level requirement
         * - Has base dexterity requirement
         * - Has base intelligence requirement
         * - Has base strength requirement
         * - Has implicit stat text
         * Printout weapons:
         * - Has base minimum physical damage
         * - Has base maximum physical damage
         * - Has base critical strike chance
         * - Has base attack speed
         * - Has base weapon range
         * Printout armors:
         * - Has base block
         * - Has base armour
         * - Has base evasion
         * - Has base energy shield
         * Possibly useful printouts not yet used:
         * - Has inventory height
         * - Has inventory width
         * - Is drop enabled
         * - Is corrupted
         * Possible item classes not used here:
         * - Life Flasks, Mana Flasks, Hybrid Flasks, Utility Flasks, Critical Utility Flasks,
         * - Currency, Stackable Currency,
         * - Active Skill Gems, Support Skill Gems,
         * - Small Relics, Medium Relics, Large Relics,
         * - Quest Items, 
         * - Maps, Map Fragments,
         * - Unarmed, Fishing Rods,
         * - Hideout Doodads, Microtransactions, Divination Card,
         * - Labyrinth Item, Labyrinth Trinket, Labyrinth Map Item
         */

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemDataLoader));

        private static readonly Regex NumberRegex = new Regex(@"\d+(\.\d+)?");
        // Links in stat texts are replaced by their second group (first: linked page title, second: text)
        private static readonly Regex LinkRegex = new Regex(@"\[\[([\w\s\d]+\|)?([\w\s\d]+)\]\]");

        // printouts for different ItemCategories
        private static readonly IReadOnlyList<string> GlobalPredicates = new[]
        {
            RdfName, RdfLvlReq, RdfBaseDexReq, RdfBaseIntReq, RdfBaseStrReq, RdfImplicits
        };
        private static readonly IReadOnlyDictionary<ItemCategory, IReadOnlyList<string>> PredicatesPerCategory
            = new Dictionary<ItemCategory, IReadOnlyList<string>>
            {
                {ItemCategory.Weapon, new[] {RdfBasePhysMin, RdfBasePhysMax, RdfBaseCritChance, RdfBaseAttackSpeed, RdfBaseWeaponRange}},
                {ItemCategory.Armour, new[] {RdfBaseBlock, RdfBaseArmour, RdfBaseEnergyShield, RdfBaseEvasion}},
                {ItemCategory.Other, new string[0]}
            };

        // the wiki's item classes are mapped to either ItemType or ItemGroup
        private static readonly IReadOnlyDictionary<string, ItemType> WikiClassToType
            = new Dictionary<string, ItemType>
            {
                {"One Hand Axes", ItemType.OneHandedAxe},
                {"Two Hand Axes", ItemType.TwoHandedAxe},
                {"Bows", ItemType.Bow},
                {"Claws", ItemType.Claw},
                {"Daggers", ItemType.Dagger},
                {"One Hand Maces", ItemType.OneHandedMace},
                {"Sceptres", ItemType.Sceptre},
                {"Two Hand Maces", ItemType.TwoHandedMace},
                {"Staves", ItemType.Staff},
                {"One Hand Swords", ItemType.OneHandedSword},
                {"Thrusting One Hand Swords", ItemType.ThrustingOneHandedSword},
                {"Two Hand Swords", ItemType.TwoHandedSword},
                {"Wands", ItemType.Wand},
                {"Amulets", ItemType.Amulet},
                {"Belts", ItemType.Belt},
                {"Quivers", ItemType.Quiver},
                {"Rings", ItemType.Ring}
            };
        private static readonly IReadOnlyDictionary<string, ItemGroup> WikiClassToGroup
            = new Dictionary<string, ItemGroup>
            {
                {"Body Armours", ItemGroup.BodyArmour},
                {"Boots", ItemGroup.Boots},
                {"Gloves", ItemGroup.Gloves},
                {"Helmets", ItemGroup.Helmet},
                {"Shields", ItemGroup.Shield},
                {"Jewel", ItemGroup.Jewel}
            };

        protected override async Task LoadAsync()
        {
            // start tasks
            var tasks = new List<Task<IEnumerable<XmlItemBase>>>();
            foreach (var pair in WikiClassToType)
            {
                var type = pair.Value;
                tasks.Add(ReadJson(pair.Key, _ => type, GroupToCategory(type.Group())));
            }
            foreach (var pair in WikiClassToGroup)
            {
                var group = pair.Value;
                tasks.Add(ReadJson(pair.Key, b => TypeFromItem(b, group), GroupToCategory(group)));
            }
            // collect results
            var bases = new List<XmlItemBase>();
            foreach (var task in tasks)
            {
                bases.AddRange(await task);
            }
            Data = new XmlItemList
            {
                ItemBases = bases.ToArray()
            };
        }

        private async Task<IEnumerable<XmlItemBase>> ReadJson(string wikiClass,
            Func<XmlItemBase, ItemType> itemTypeFunc, ItemCategory category)
        {
            var conditions = new ConditionBuilder
            {
                {RdfRarity, "Normal"},
                {RdfDropEnabled, "true"},
                {RdfItemClass, wikiClass}
            };
            var printouts = GlobalPredicates.Union(PredicatesPerCategory[category]);
            var enumerable =
                from result in await WikiApiAccessor.AskArgs(conditions, printouts)
                select PrintoutsToBase(category, result);
            var ret = enumerable.ToList();
            foreach (var item in ret)
            {
                item.ItemType = itemTypeFunc(item);
            }
            Log.Info($"Retrieved {ret.Count} bases of class {wikiClass}.");
            return
                from b in ret
                orderby b.ItemType, b.Level, b.Name
                select b;
        }

        private static XmlItemBase PrintoutsToBase(ItemCategory category, JToken printouts)
        {
            // name, requirements and implicts; same for all categories
            var implicits = PluralValue<string>(printouts, RdfImplicits).SelectMany(ConvertStatText).ToArray();
            var item = new XmlItemBase
            {
                Level = SingularValue<int>(printouts, RdfLvlReq),
                Dexterity = SingularValue(printouts, RdfBaseDexReq, 0),
                Intelligence = SingularValue(printouts, RdfBaseIntReq, 0),
                Strength = SingularValue(printouts, RdfBaseStrReq, 0),
                Name = SingularValue<string>(printouts, RdfName),
                Implicit = implicits.Any() ? implicits : null
            };
            // properties; category specific
            var propBuilder = new PropertyBuilder(printouts);
            switch (category)
            {
                case ItemCategory.Weapon:
                    propBuilder.Add("Physical Damage", RdfBasePhysMin, RdfBasePhysMax);
                    propBuilder.Add("Critical Strike Chance %", RdfBaseCritChance);
                    propBuilder.Add("Attacks per Second", RdfBaseAttackSpeed);
                    propBuilder.Add("Weapon Range", RdfBaseWeaponRange);
                    break;
                case ItemCategory.Armour:
                    propBuilder.Add("Chance to Block %", RdfBaseBlock);
                    propBuilder.Add("Armour", RdfBaseArmour);
                    propBuilder.Add("Evasion Rating", RdfBaseEvasion);
                    propBuilder.Add("Energy Shield", RdfBaseEnergyShield);
                    break;
                case ItemCategory.Other:
                    break;
            }
            item.Properties = propBuilder.ToArray();
            return item;
        }

        private static IEnumerable<XmlStat> ConvertStatText(string statText)
        {
            // split text at "<br>", replace links, convert each to XmlStat
            return from raw in statText.Split(new[] {"<br>"}, StringSplitOptions.RemoveEmptyEntries)
                   let filtered = LinkRegex.Replace(raw, "$2")
                   from s in ConvertStat(filtered)
                   select s;
        }

        private static IEnumerable<XmlStat> ConvertStat(string stat)
        {
            var matches = NumberRegex.Matches(stat);
            if (matches.Count <= 0)
            {
                // no numbers in stat, easy
                yield return new XmlStat {Name = stat};
                yield break;
            }

            stat = NumberRegex.Replace(stat, "#");
            // range: first value is From, second is To
            const string range = "(#-#)";
            // added damage: first value is minimum, second is maximum
            const string addNoRange = "# to #";
            // added damage with range
            const string addRange = range + " to " + range;
            if (stat.Contains(addNoRange))
            {
                if (matches.Count != 2)
                {
                    Log.Warn($"Could not parse implicit {stat}");
                    yield break;
                }
                // stat contains "#1 to #2", convert to two stats:
                // "#1 minimum" and "#2 maximum"
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = stat.Replace(addNoRange, "# minimum")
                };
                from = matches[1].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = from,
                    Name = stat.Replace(addNoRange, "# maximum")
                };
            }
            else if (stat.Contains(addRange))
            {
                if (matches.Count != 4)
                {
                    Log.Warn($"Could not parse implicit {stat}");
                    yield break;
                }
                // stat contains "(#1-#2) to (#3-#4)" convert to two stats:
                // "(#1-#2) minimum" and "(#3-#4) maximum"
                yield return new XmlStat
                {
                    From = matches[0].Value.ParseFloat(),
                    To = matches[1].Value.ParseFloat(),
                    Name = stat.Replace(addRange, "# minimum")
                };
                yield return new XmlStat
                {
                    From = matches[2].Value.ParseFloat(),
                    To = matches[3].Value.ParseFloat(),
                    Name = stat.Replace(addRange, "# maximum")
                };
            }
            else
            {
                // stat contains "#1" or "(#1-#2)
                var from = matches[0].Value.ParseFloat();
                yield return new XmlStat
                {
                    From = from,
                    To = matches.Count > 1 ? matches[1].Value.ParseFloat() : from,
                    Name = stat.Replace(range, "#")
                };
            }
        }

        private static ItemCategory GroupToCategory(ItemGroup group)
        {
            switch (group)
            {
                case ItemGroup.OneHandedWeapon:
                case ItemGroup.TwoHandedWeapon:
                    return ItemCategory.Weapon;
                case ItemGroup.BodyArmour:
                case ItemGroup.Boots:
                case ItemGroup.Gloves:
                case ItemGroup.Helmet:
                case ItemGroup.Shield:
                    return ItemCategory.Armour;
                case ItemGroup.Belt:
                case ItemGroup.Ring:
                case ItemGroup.Amulet:
                case ItemGroup.Quiver:
                case ItemGroup.Jewel:
                    return ItemCategory.Other;
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, "Unexpected group");
            }
        }

        private static ItemType TypeFromItem(XmlItemBase item, ItemGroup group)
        {
            ItemType ret;
            var type = group.ToString();
            // If there is a type with the same name as the group, take that
            if (Enum.TryParse(type, out ret))
                return ret;

            // Each jewel base has its own type.
            if (Enum.TryParse(item.Name.Replace(" Jewel", "Jewel"), out ret))
                return ret;

            // Types of armour groups.
            if (item.Strength > 0)
            {
                type += "Armour";
            }
            if (item.Dexterity > 0)
            {
                type += "Evasion";
            }
            if (item.Intelligence > 0)
            {
                type += "EnergyShield";
            }
            // Special case for when all of Str, Dex and Int are 0
            if (type == group.ToString())
            {
                type += "ArmourEvasionEnergyShield";
            }
            if (!Enum.TryParse(type, out ret))
                throw new ArgumentException($"Type {type} parsed from the given base item and group does not exist");
            return ret;
        }


        private enum ItemCategory
        {
            Weapon,
            Armour,
            Other
        }


        private class PropertyBuilder
        {
            private readonly JToken _printouts;
            private readonly List<XmlStat> _properties = new List<XmlStat>();

            public PropertyBuilder(JToken printouts)
            {
                _printouts = printouts;
            }

            public XmlStat[] ToArray()
            {
                return _properties.Any() ? _properties.ToArray() : null;
            }

            public void Add(string name, string rdfPredicate)
            {
                Add(name, rdfPredicate, rdfPredicate);
            }

            public void Add(string name, string rdfPredicateFrom, string rdfPredicateTo)
            {
                var from = SingularValue<float>(_printouts, rdfPredicateFrom, 0);
                var to = SingularValue<float>(_printouts, rdfPredicateTo, 0);
                if (from.AlmostEquals(0, 0.001F) && to.AlmostEquals(0, 0.001F)) // stats don't use many decimal places
                    return;
                _properties.Add(new XmlStat
                {
                    Name = name,
                    From = from,
                    To = to
                });
            }
        }
    }
}