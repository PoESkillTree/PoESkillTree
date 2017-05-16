using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
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
         * - Has tags
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
         * Possible item classes not used here:
         * - Life Flasks, Mana Flasks, Hybrid Flasks, Utility Flasks, Critical Utility Flasks,
         * - Currency, Stackable Currency,
         * - Active Skill Gems, Support Skill Gems,
         * - Small Relics, Medium Relics, Large Relics,
         * - Quest Items, 
         * - Maps, Map Fragments,
         * - Unarmed,
         * - Hideout Doodads, Microtransactions, Divination Card,
         * - Labyrinth Item, Labyrinth Trinket, Labyrinth Map Item
         */

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemBaseLoader));
        
        private static readonly IReadOnlyList<string> PrintoutPredicates = new[]
        {
            // Global
            RdfName, RdfLvlReq, RdfBaseDexReq, RdfBaseIntReq, RdfBaseStrReq, RdfImplicits, RdfDropEnabled,
            RdfInventoryHeight, RdfInventoryWidth, RdfMetadataId, RdfTags,
            // Weapon
            RdfBasePhysMin, RdfBasePhysMax, RdfBaseCritChance, RdfBaseAttackSpeed, RdfBaseWeaponRange,
            // Armour
            RdfBaseBlock, RdfBaseArmour, RdfBaseEnergyShield, RdfBaseEvasion
        };

        // the wiki's item classes are mapped to ItemClass 
        // (Wiki uses ItemClass.Name, we use ItemClass.Id with code naming convention)
        private static readonly IReadOnlyDictionary<string, ItemClass> WikiClassToItemClass =
            new Dictionary<string, ItemClass>
            {
                { "One Hand Swords", ItemClass.OneHandSword },
                { "Thrusting One Hand Swords", ItemClass.ThrustingOneHandSword },
                { "One Hand Axes", ItemClass.OneHandAxe },
                { "One Hand Maces", ItemClass.OneHandMace },
                { "Sceptres", ItemClass.Sceptre },
                { "Daggers", ItemClass.Dagger },
                { "Claws", ItemClass.Claw },
                { "Wands", ItemClass.Wand },
                { "Fishing Rods", ItemClass.FishingRod },

                { "Two Hand Swords", ItemClass.TwoHandSword },
                { "Two Hand Axes", ItemClass.TwoHandAxe },
                { "Two Hand Maces", ItemClass.TwoHandMace },
                { "Bows", ItemClass.Bow },
                { "Staves", ItemClass.Staff },

                { "Belts", ItemClass.Belt },
                { "Rings", ItemClass.Ring },
                { "Amulets", ItemClass.Amulet },
                { "Quivers", ItemClass.Quiver },

                { "Shields", ItemClass.Shield },
                { "Boots", ItemClass.Boots },
                { "Body Armours", ItemClass.BodyArmour },
                { "Gloves", ItemClass.Gloves },
                { "Helmets", ItemClass.Helmet },

                { "Jewel", ItemClass.Jewel },
            };

        private readonly HashSet<string> _unknownTags = new HashSet<string>();

        protected override async Task LoadAsync()
        {
            // start tasks
            var tasks = new List<Task<IEnumerable<XmlItemBase>>>();
            foreach (var pair in WikiClassToItemClass)
            {
                tasks.Add(ReadJson(pair.Key, pair.Value));
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
            Log.Info("Unknown tags: " + string.Join(", ", _unknownTags));
        }

        private async Task<IEnumerable<XmlItemBase>> ReadJson(string wikiClass, ItemClass itemClass)
        {
            var conditions = new ConditionBuilder
            {
                {RdfRarity, "Normal"},
                {RdfItemClass, wikiClass}
            };
            var enumerable =
                from result in await WikiApiAccessor.AskArgs(conditions, PrintoutPredicates)
                select PrintoutsToBase(itemClass, result);
            var ret = enumerable.ToList();
            Log.Info($"Retrieved {ret.Count} bases of class {wikiClass}.");
            return
                from b in ret
                orderby b.Level, b.Name
                select b;
        }

        private XmlItemBase PrintoutsToBase(ItemClass itemClass, JToken printouts)
        {
            // name, requirements and implicts; same for all categories
            var implicits = PluralValue<string>(printouts, RdfImplicits)
                .Select(s => new XmlMod {Id = s}).ToArray();
            var item = new XmlItemBase
            {
                Level = SingularValue<int>(printouts, RdfLvlReq),
                Dexterity = SingularValue(printouts, RdfBaseDexReq, 0),
                Intelligence = SingularValue(printouts, RdfBaseIntReq, 0),
                Strength = SingularValue(printouts, RdfBaseStrReq, 0),
                Name = SingularValue<string>(printouts, RdfName),
                DropDisabled = !SingularBool(printouts, RdfDropEnabled, true),
                InventoryHeight = SingularValue(printouts, RdfInventoryHeight, 1),
                InventoryWidth = SingularValue(printouts, RdfInventoryWidth, 1),
                MetadataId = SingularValue<string>(printouts, RdfMetadataId),
                Implicit = implicits,
                ItemClass = itemClass,
            };
            // tags and properties
            foreach (var s in PluralValue<string>(printouts, RdfTags))
            {
                Tags tag;
                if (TagsEx.TryParse(s, out tag))
                {
                    item.Tags |= tag;
                }
                else
                {
                    _unknownTags.Add(s);
                }
            }
            // properties; tag specific
            var propBuilder = new PropertyBuilder(printouts);
            if (item.Tags.HasFlag(Tags.Weapon))
            {
                propBuilder.Add("Physical Damage", RdfBasePhysMin, RdfBasePhysMax);
                propBuilder.Add("Critical Strike Chance %", RdfBaseCritChance);
                propBuilder.Add("Attacks per Second", RdfBaseAttackSpeed);
                propBuilder.Add("Weapon Range", RdfBaseWeaponRange);
            }
            if (item.Tags.HasFlag(Tags.Armour))
            {
                propBuilder.Add("Chance to Block %", RdfBaseBlock);
                propBuilder.Add("Armour", RdfBaseArmour);
                propBuilder.Add("Evasion Rating", RdfBaseEvasion);
                propBuilder.Add("Energy Shield", RdfBaseEnergyShield);
            }
            item.Properties = propBuilder.ToArray();
            return item;
        }
    }
}