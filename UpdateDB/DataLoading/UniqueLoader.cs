using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MoreLinq;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Utils.WikiApi;
using static POESKillTree.Utils.WikiApi.CargoConstants;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves unique items from the Wiki through its API.
    /// </summary>
    public class UniqueLoader : XmlDataLoader<XmlUniqueList>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UniqueLoader));

        private const string PageNameAlias = "page_title";

        private static readonly IReadOnlyList<string> Fields = new[]
        {
            Name, BaseItemId, RequiredLevel, ExplicitMods, DropEnabled, $"{ItemTableName}.{PageName}={PageNameAlias}"
        };

        private static readonly IReadOnlyList<string> JewelFields = Fields.Concat(new[]
        {
            JewelLimit
        }).ToList();

        private static readonly IReadOnlyList<string> RelevantWikiClasses = new[]
        {
            "One Hand Axes", "Two Hand Axes", "Bows", "Claws", "Daggers",
            "One Hand Maces", "Sceptres", "Two Hand Maces", "Staves",
            "One Hand Swords", "Thrusting One Hand Swords", "Two Hand Swords", "Wands",
            "Amulets", "Belts", "Quivers", "Rings",
            "Body Armours", "Boots", "Helmets", "Gloves", "Shields",
        };

        private const string JewelClass = "Jewel";

        protected override async Task LoadAsync()
        {
            var uniques = new List<XmlUnique>();
            foreach (var wikiClass in RelevantWikiClasses)
            {
                uniques.AddRange(await LoadAsync(wikiClass));
            }
            uniques.AddRange(await LoadJewelsAsync());
            Data = new XmlUniqueList
            {
                Uniques = uniques.ToArray()
            };
        }

        private async Task<IEnumerable<XmlUnique>> LoadAsync(string wikiClass)
        {
            var results = await QueryApiAsync(wikiClass);
            return ReadJson(wikiClass, results);
        }

        private async Task<IEnumerable<XmlUnique>> LoadJewelsAsync()
        {
            var results = await QueryApiForJewelsAsync();
            return ReadJson(JewelClass, results);
        }

        private Task<IEnumerable<JToken>> QueryApiAsync(string wikiClass)
        {
            string[] tables = { ItemTableName };
            var whereBuilder = new WhereBuilder()
                .Add(Rarity, "Unique")
                .Add(ItemClass, wikiClass);
            return WikiApiAccessor.CargoQuery(tables, Fields, whereBuilder.ToString());
        }

        private Task<IEnumerable<JToken>> QueryApiForJewelsAsync()
        {
            string[] tables = { ItemTableName, JewelTableName };
            var whereBuilder = new WhereBuilder()
                .Add(Rarity, "Unique")
                .Add(ItemClass, JewelClass);
            var joinOn = $"{ItemTableName}.{PageName}={JewelTableName}.{PageName}";
            return WikiApiAccessor.CargoQuery(tables, JewelFields, whereBuilder.ToString(), joinOn);
        }

        private static IEnumerable<XmlUnique> ReadJson(string wikiClass, IEnumerable<JToken> results)
        {
            var enumerable =
                from result in results
                let pageName = result.Value<string>(PageNameAlias)
                let unique = PrintoutsToUnique(result)
                orderby unique.Name
                select new {pageName, unique};
            var ret = enumerable.DistinctBy(x => x.pageName).Select(x => x.unique).ToList();
            Log.Info($"Retrieved {ret.Count} uniques of class {wikiClass}.");
            return ret;
        }

        private static XmlUnique PrintoutsToUnique(JToken printouts)
        {
            string[] explicits = printouts.Value<string>(ExplicitMods)
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var properties = new List<string>();
            if (printouts[JewelLimit]?.Value<string>() is string itemLimit && itemLimit.Length > 0)
            {
                properties.Add($"Limited to: {itemLimit}");
            }

            return new XmlUnique
            {
                Level = int.Parse(printouts.Value<string>(RequiredLevel)),
                Name = printouts.Value<string>(Name),
                DropDisabled = printouts.Value<string>(DropEnabled) == "0",
                BaseMetadataId = printouts.Value<string>(BaseItemId),
                Explicit = explicits,
                Properties = properties.ToArray()
            };
        }
    }
}