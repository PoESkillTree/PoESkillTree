using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items;
using static POESKillTree.Utils.WikiApi.CargoConstants;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves unique items from the Wiki through its API.
    /// </summary>
    public class UniqueLoader : XmlDataLoader<XmlUniqueList>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UniqueLoader));

        private static readonly IReadOnlyList<string> Fields = new[]
        {
            Name, BaseItemId, RequiredLevel, ExplicitMods, DropEnabled
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
            var where = GetWhereClause(wikiClass);
            return WikiApiAccessor.CargoQueryAsync(tables, Fields, where);
        }

        private Task<IEnumerable<JToken>> QueryApiForJewelsAsync()
        {
            string[] tables = { ItemTableName, JewelTableName };
            var where = GetWhereClause(JewelClass);
            var joinOn = $"{ItemTableName}.{PageName}={JewelTableName}.{PageName}";
            return WikiApiAccessor.CargoQueryAsync(tables, JewelFields, where, joinOn);
        }

        private static string GetWhereClause(string wikiClass)
        {
            return $"{Rarity}='Unique' AND {ItemClass}='{wikiClass}'";
        }

        private static IEnumerable<XmlUnique> ReadJson(string wikiClass, IEnumerable<JToken> results)
        {
            List<XmlUnique> uniques = (
                from result in results
                let unique = PrintoutsToUnique(result)
                orderby unique.Name
                select unique
            ).ToList();
            Log.Info($"Retrieved {uniques.Count} uniques of class {wikiClass}.");
            return uniques;
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