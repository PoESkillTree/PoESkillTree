using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Items;
using POESKillTree.Utils.Extensions;

namespace UpdateEquipment.Utils
{
    public class WikiUtils
    {
        public const string WikiUrlPrefix = "http://pathofexile.gamepedia.com/";

        private static readonly ILog Log = LogManager.GetLogger(typeof(WikiUtils));

        /// <summary>
        /// Contains all wiki pages that must be scraped and the item types that can be found in them
        /// (in the order in which they occur).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> WikiUrls = new Dictionary<string, IReadOnlyList<ItemType>>
        {
            {"List_of_axes", new []{ItemType.OneHandedAxe, ItemType.TwoHandedAxe}},
            {"List_of_bows", new []{ItemType.Bow}},
            {"List_of_claws", new []{ItemType.Claw}},
            {"List_of_daggers", new []{ItemType.Dagger}},
            {"List_of_maces", new []{ItemType.OneHandedMace, ItemType.Sceptre, ItemType.TwoHandedMace}},
            {"List_of_staves", new []{ItemType.Staff}},
            {"List_of_swords", new []{ItemType.OneHandedSword, ItemType.ThrustingOneHandedSword, ItemType.TwoHandedSword}},
            {"List_of_wands", new []{ItemType.Wand}},

            {"List_of_amulets", new []{ItemType.Amulet}},
            {"List_of_belts", new []{ItemType.Belt}},
            {"List_of_quivers", new []{ItemType.Quiver, ItemType.Quiver}}, // current quivers and old quivers
            {"List_of_rings", new []{ItemType.Ring}},

            {"Body_armour", ItemGroup.BodyArmour.Types()},
            {"Boots", ItemGroup.Boots.Types()},
            {"Gloves", ItemGroup.Gloves.Types()},
            {"Helmet", ItemGroup.Helmet.Types()},
            {"Shield", ItemGroup.Shield.Types()}
        };

        private readonly CachingHttpClient _httpClient;

        public WikiUtils(CachingHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task ForEachBaseItemAsync(Action<HtmlNode, ItemType> tableParsingFunc)
        {
            foreach (var tablesTask in WikiUrls.Select(pair => LoadTableAsync(pair.Key, pair.Value)))
            {
                var tables = await tablesTask;
                tables.ForEach(b => tableParsingFunc(b.HtmlTable, b.ItemType));
            }
        }

        public async Task<List<T>> SelectFromBaseItemsAsync<T>(Func<HtmlNode, ItemType, IEnumerable<T>> tableParsingFunc)
        {
            var bases = new List<T>();
            foreach (var tablesTask in WikiUrls.Select(pair => LoadTableAsync(pair.Key, pair.Value)))
            {
                var tables = await tablesTask;
                bases.AddRange(tables.Select(b => tableParsingFunc(b.HtmlTable, b.ItemType)).Flatten());
            }
            return bases;
        }

        private async Task<IList<BaseItemTable>> LoadTableAsync(string urlSuffix, IReadOnlyCollection<ItemType> itemTypes)
        {
            var doc = new HtmlDocument();
            var file = await _httpClient.GetStringAsync(WikiUrlPrefix + urlSuffix);
            doc.LoadHtml(file);

            var tables =
                doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]")
                    .Where(node => node.SelectNodes("tr[1]/th[1 and . = \"Name\"]") != null).ToList();
            if (itemTypes.Count > tables.Count)
            {
                Log.WarnFormat("Not enough tables found in {0} for the number of item types that should be there.", urlSuffix);
                Log.WarnFormat("Skipping item types {0}", itemTypes);
                return new List<BaseItemTable>();
            }
            // Only the first itemType.Count tables are parsed.
            return tables.Take(itemTypes.Count).Zip(itemTypes, BaseItemTable.Create).ToList();
        }

        private class BaseItemTable
        {
            public HtmlNode HtmlTable { get; private set; }
            public ItemType ItemType { get; private set; }

            public static BaseItemTable Create(HtmlNode htmlTable, ItemType itemType)
            {
                return new BaseItemTable
                {
                    HtmlTable = htmlTable,
                    ItemType = itemType
                };
            }
        }
    }
}