using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils
{
    public class WikiUtils
    {
        public const double ItemImageResizeFactor = 0.6;

        private const string WikiUrlPrefix = "http://pathofexile.gamepedia.com/";

        private static readonly ILog Log = LogManager.GetLogger(typeof(WikiUtils));

        /// <summary>
        /// Contains all wiki pages that contain base items and the item types that can be found in them
        /// (in the order in which they occur).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> BaseItemTableUrls = new Dictionary<string, IReadOnlyList<ItemType>>
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

        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> GemTableUrls = new Dictionary<string, IReadOnlyList<ItemType>>
        {
            {"List_of_active_skill_gems", new []{ItemType.Gem, ItemType.Gem, ItemType.Gem, ItemType.Gem}}, // Str, Dex, Int, White
            {"List_of_support_skill_gems", new []{ItemType.Gem, ItemType.Gem, ItemType.Gem}} // Str, Dex, Int
        };

        private readonly Func<string, Task<string>> _getStringAsync;

        public WikiUtils(HttpClient httpClient)
        {
            _getStringAsync = httpClient.GetStringAsync;
        }

        public Task<List<T>> SelectFromGemsAsync<T>(Func<HtmlNode, ItemType, IEnumerable<T>> tableParsingFunc)
        {
            return SelectFromItemTablesAsync(GemTableUrls, tableParsingFunc);
        }

        public Task ForEachBaseItemAsync(Action<HtmlNode, ItemType> tableParsingFunc)
        {
            return ForEachItemTableAsync(BaseItemTableUrls, tableParsingFunc);
        }

        public Task<List<T>> SelectFromBaseItemsAsync<T>(Func<HtmlNode, ItemType, IEnumerable<T>> tableParsingFunc)
        {
            return SelectFromItemTablesAsync(BaseItemTableUrls, tableParsingFunc);
        }

        private async Task ForEachItemTableAsync(IReadOnlyDictionary<string, IReadOnlyList<ItemType>> itemTables,
            Action<HtmlNode, ItemType> tableParsingFunc)
        {
            foreach (var tablesTask in itemTables.Select(pair => LoadTableAsync(pair.Key, pair.Value)))
            {
                var tables = await tablesTask.ConfigureAwait(false);
                tables.ForEach(b => tableParsingFunc(b.HtmlTable, b.ItemType));
            }
        }

        private async Task<List<T>> SelectFromItemTablesAsync<T>(
            IReadOnlyDictionary<string, IReadOnlyList<ItemType>> itemTables,
            Func<HtmlNode, ItemType, IEnumerable<T>> tableParsingFunc)
        {
            var bases = new List<T>();
            foreach (var tablesTask in itemTables.Select(pair => LoadTableAsync(pair.Key, pair.Value)))
            {
                var tables = await tablesTask.ConfigureAwait(false);
                bases.AddRange(tables.Select(b => tableParsingFunc(b.HtmlTable, b.ItemType)).Flatten());
            }
            return bases;
        }

        private async Task<IList<BaseItemTable>> LoadTableAsync(string urlSuffix, IReadOnlyCollection<ItemType> itemTypes)
        {
            var doc = new HtmlDocument();
            var file = await _getStringAsync(WikiUrlPrefix + urlSuffix).ConfigureAwait(false);
            doc.LoadHtml(file);

            var tables =
                doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]")
                    .Where(node => node.SelectNodes("tr[1]/th[1 and (. = \"Name\" or . = \"Skill gem\")]") != null).ToList();
            if (itemTypes.Count > tables.Count)
            {
                Log.WarnFormat("Not enough tables found in {0} for the number of item types that should be there.", urlSuffix);
                Log.WarnFormat("Skipping item types {{{0}}}", string.Join(", ", itemTypes));
                return new List<BaseItemTable>();
            }
            // Only the first itemType.Count tables are parsed.
            return tables.Take(itemTypes.Count).Zip(itemTypes, BaseItemTable.Create).ToList();
        }

        public async Task<Tuple<string, Uri>> LoadItemBoxImageAsync(string urlSuffix)
        {
            var doc = new HtmlDocument();
            var file = await _getStringAsync(WikiUrlPrefix + urlSuffix).ConfigureAwait(false);
            doc.LoadHtml(file);
            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'item-box')]/div[contains(@class, 'image')]/a/img");
            var node = nodes[0];
            return Tuple.Create(node.GetAttributeValue("alt", ""), new Uri(node.GetAttributeValue("src", "")));
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