using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ItemBaseLoader : XmlDataLoader<XmlItemList>
    {
        /* Conditions:
         * - Has rarity::Normal
         * - Has item class::{itemClass}
         * Printout global:
         * - Has name
         * - Has level requirement
         * - Has base dexterity requirement
         * - Has base intelligence requirement
         * - Has base strength requirement
         * - Has implicit stat text
         * - Is drop enabled
         * - Has inventory height
         * - Has inventory width
         * - Has metadata id
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
         * - Is corrupted
         * - Has tags
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemBaseLoader));

        // printouts for different ItemCategories
        private static readonly IReadOnlyList<string> GlobalPredicates = new[]
        {
            RdfName, RdfLvlReq, RdfBaseDexReq, RdfBaseIntReq, RdfBaseStrReq, RdfImplicits, RdfDropEnabled,
            RdfInventoryHeight, RdfInventoryWidth, RdfMetadataId
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
            var implicits = PluralValue<string>(printouts, RdfImplicits)
                .SelectMany(WikiStatTextUtils.ConvertStatText).ToArray();
            var item = new XmlItemBase
            {
                Level = SingularValue<int>(printouts, RdfLvlReq),
                Dexterity = SingularValue(printouts, RdfBaseDexReq, 0),
                Intelligence = SingularValue(printouts, RdfBaseIntReq, 0),
                Strength = SingularValue(printouts, RdfBaseStrReq, 0),
                Name = SingularValue<string>(printouts, RdfName),
                DropDisabled = !SingularBool(printouts, RdfDropEnabled),
                InventoryHeight = SingularValue(printouts, RdfInventoryHeight, 1),
                InventoryWidth = SingularValue(printouts, RdfInventoryWidth, 1),
                MetadataId = SingularValue<string>(printouts, RdfMetadataId),
                Implicit = implicits,
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
                return _properties.ToArray();
            }

            public void Add(string name, string rdfPredicate)
            {
                Add(name, rdfPredicate, rdfPredicate);
            }

            public void Add(string name, string rdfPredicateFrom, string rdfPredicateTo)
            {
                var from = SingularValue<float>(_printouts, rdfPredicateFrom, 0);
                var to = SingularValue<float>(_printouts, rdfPredicateTo, 0);
                if (from.AlmostEquals(0, 0.001) && to.AlmostEquals(0, 0.001)) // stats don't use many decimal places
                    return;
                _properties.Add(new XmlStat
                {
                    Name = name,
                    From = new[] { from },
                    To = new[] { to }
                });
            }
        }
    }
}