using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Utils.WikiApi;
using static POESKillTree.Utils.WikiApi.WikiApiUtils;
using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves unique items from the Wiki through its API.
    /// </summary>
    public class UniqueLoader : XmlDataLoader<XmlUniqueList>
    {
        /* Conditions:
         * - Has rarity::Unique
         * - Has item class::{itemClass}
         * Printout:
         * - Has name
         * - Has base item metadata id
         * - Has level requirement
         * - Has explicit mod ids
         * - Is drop enabled
         * Printout Jewel:
         * - Has item limit
         * - Has jewel radius
         * Possibly useful printouts not yet used:
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

        private static readonly ILog Log = LogManager.GetLogger(typeof(UniqueLoader));

        private static readonly IReadOnlyList<string> Printouts = new[]
        {
            RdfName, RdfBaseMetadataId, RdfLvlReq, RdfExplicits, RdfDropEnabled
        };

        private static readonly IReadOnlyList<string> PrintoutsJewel = new[]
        {
            RdfItemLimit, RdfJewelRadius
        };

        private static readonly IReadOnlyList<string> RelevantWikiClasses = new[]
        {
            "One Hand Axes", "Two Hand Axes", "Bows", "Claws", "Daggers",
            "One Hand Maces", "Sceptres", "Two Hand Maces", "Staves",
            "One Hand Swords", "Thrusting One Hand Swords", "Two Hand Swords", "Wands",
            "Amulets", "Belts", "Quivers", "Rings",
            "Body Armours", "Boots", "Helmets", "Gloves", "Shields", "Jewel",
        };

        protected override async Task LoadAsync()
        {
            var tasks = RelevantWikiClasses.Select(ReadJson).ToList();
            var uniques = new List<XmlUnique>();
            foreach (var task in tasks)
            {
                uniques.AddRange(await task);
            }
            Data = new XmlUniqueList
            {
                Uniques = uniques.ToArray()
            };
        }

        private async Task<IEnumerable<XmlUnique>> ReadJson(string wikiClass)
        {
            var conditions = new ConditionBuilder
            {
                {RdfRarity, "Unique"},
                {RdfItemClass, wikiClass}
            };
            var printouts = wikiClass == "Jewel"
                ? Printouts.Concat(PrintoutsJewel)
                : Printouts;
            IEnumerable<XmlUnique> enumerable =
                from result in await WikiApiAccessor.AskArgs(conditions, printouts)
                select PrintoutsToUnique(result);
            var ret = enumerable.ToList();
            Log.Info($"Retrieved {ret.Count} bases of class {wikiClass}.");
            return ret.OrderBy(b => b.Name);
        }

        private static XmlUnique PrintoutsToUnique(JToken printouts)
        {
            var explicits = PluralValue<string>(printouts, RdfExplicits).ToArray();
            var properties = new PropertyBuilder(printouts);
            if (printouts[RdfItemLimit]?.HasValues == true)
            {
                properties.Add("Limited to: {0}", RdfItemLimit);
            }
            if (printouts[RdfJewelRadius]?.HasValues == true)
            {
                properties.Add("Radius: {0}", RdfJewelRadius);
            }
            return new XmlUnique
            {
                Level = SingularValue<int>(printouts, RdfLvlReq),
                Name = SingularValue<string>(printouts, RdfName),
                DropDisabled = !SingularBool(printouts, RdfDropEnabled, true),
                BaseMetadataId = SingularValue<string>(printouts, RdfBaseMetadataId),
                Explicit = explicits,
                Properties = properties.ToArray()
            };
        }
    }
}