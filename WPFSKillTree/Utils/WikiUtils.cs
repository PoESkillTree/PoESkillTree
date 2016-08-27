using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Provides methods for extracting data from the unofficial PoE Wiki at Gamepedia.
    /// </summary>
    public class WikiUtils
    {
        /// <summary>
        /// The factor by which item images from the Wiki have to be resized to fit into the inventory/stash slots.
        /// </summary>
        public const double ItemImageResizeFactor = 0.6;

        private const string WikiUrlPrefix = "http://pathofexile.gamepedia.com/";

        private static readonly ILog Log = LogManager.GetLogger(typeof(WikiUtils));

        /// <summary>
        /// Contains all wiki pages that contain base items and the item types that can be found in them.
        /// (in the order in which they occur, each item type references one table).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> BaseItemTableUrls =
            new Dictionary<string, IReadOnlyList<ItemType>>
        {
            {"List_of_axes", new[] {ItemType.OneHandedAxe, ItemType.TwoHandedAxe}},
            {"List_of_bows", new[] {ItemType.Bow}},
            {"List_of_claws", new[] {ItemType.Claw}},
            {"List_of_daggers", new[] {ItemType.Dagger}},
            {"List_of_maces", new[] {ItemType.OneHandedMace, ItemType.Sceptre, ItemType.TwoHandedMace}},
            {"List_of_staves", new[] {ItemType.Staff}},
            {
                "List_of_swords",
                new[] {ItemType.OneHandedSword, ItemType.ThrustingOneHandedSword, ItemType.TwoHandedSword}
            },
            {"List_of_wands", new[] {ItemType.Wand}},

            {"List_of_amulets", new[] {ItemType.Amulet}},
            {"List_of_belts", new[] {ItemType.Belt}},
            {"List_of_quivers", new[] {ItemType.Quiver, ItemType.Quiver}}, // current quivers and old quivers
            {"List_of_rings", new[] {ItemType.Ring}},

            {"Body_armour", ItemGroup.BodyArmour.Types()},
            {"Boots", ItemGroup.Boots.Types()},
            {"Gloves", ItemGroup.Gloves.Types()},
            {"Helmet", ItemGroup.Helmet.Types()},
            {"Shield", ItemGroup.Shield.Types()}
        };

        /// <summary>
        /// Contains all wiki pages that contain gems. The list has one item type for each table that should be
        /// read from (starting from the first table on the page).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<ItemType>> GemTableUrls =
            new Dictionary<string, IReadOnlyList<ItemType>>
        {
            {"List_of_skill_gems", new[] {ItemType.Gem}}
        };

        private readonly HttpClient _httpClient;

        /// <param name="httpClient">The <see cref="HttpClient"/> instance used for internet access.</param>
        public WikiUtils(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Reads the tables that contain lists of gems asynchronously, applies <paramref name="tableParsingFunc"/>
        /// to each and returns all results.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="tableParsingFunc"/></typeparam>
        /// <param name="tableParsingFunc">Parses the given wiki table containing gem information and returns a
        /// enumerable of <typeparamref name="T"/>. Has to be thread safe as it may be called multiple times in
        /// parallel.</param>
        /// <returns>Contains all <typeparamref name="T"/> instances returned by the
        /// <paramref name="tableParsingFunc"/> calls.</returns>
        public Task<IEnumerable<T>> SelectFromGemsAsync<T>(Func<HtmlNode, IEnumerable<T>> tableParsingFunc)
        {
            return SelectFromItemTablesAsync(GemTableUrls, (node, type) => tableParsingFunc(node));
        }

        /// <summary>
        /// Reads the tables that contain lists of base items asynchronously and calls
        /// <paramref name="tableParsingFunc"/> for each.
        /// </summary>
        /// <param name="tableParsingFunc">Parses the given wiki table that contains a information about items about
        /// the given types. Has to be thread safe as it may be called multiple times in parallel.</param>
        public Task ForEachBaseItemAsync(Action<HtmlNode, ItemType> tableParsingFunc)
        {
            return ForEachItemTableAsync(BaseItemTableUrls, tableParsingFunc);
        }

        /// <summary>
        /// Reads the tables that contain lists of base items asynchronously, applies
        /// <paramref name="tableParsingFunc"/> to each and returns all results.
        /// </summary>
        /// <typeparam name="T">The result type of <paramref name="tableParsingFunc"/></typeparam>
        /// <param name="tableParsingFunc">Parses the given wiki table that contains a information about items about
        /// the given types and returns a enumerable of <typeparamref name="T"/>. Has to be thread safe as it may be
        /// called multiple times in parallel.</param>
        /// <returns>Contains all <typeparamref name="T"/> instances returned by the
        /// <paramref name="tableParsingFunc"/> calls.</returns>
        public Task<IEnumerable<T>> SelectFromBaseItemsAsync<T>(
            Func<HtmlNode, ItemType, IEnumerable<T>> tableParsingFunc)
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

        private async Task<IEnumerable<T>> SelectFromItemTablesAsync<T>(
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

        private async Task<IList<BaseItemTable>> LoadTableAsync(string urlSuffix,
            IReadOnlyCollection<ItemType> itemTypes)
        {
            var doc = new HtmlDocument();
            var file = await _httpClient.GetStringAsync(WikiUrlPrefix + urlSuffix).ConfigureAwait(false);
            doc.LoadHtml(file);

            var tables =
                doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]")
                    .Where(node => node.SelectNodes("tr[1]/th[1 and (. = \"Item\" or . = \"Skill gem\")]") != null)
                    .ToList();
            if (itemTypes.Count > tables.Count)
            {
                Log.WarnFormat("Not enough tables found in {0} for the number of item types that should be there.",
                    urlSuffix);
                Log.WarnFormat("Skipping item types {{{0}}}", string.Join(", ", itemTypes));
                return new List<BaseItemTable>();
            }
            // Only the first itemType.Count tables are parsed.
            return tables.Take(itemTypes.Count).Zip(itemTypes, BaseItemTable.Create).ToList();
        }

        /// <summary>
        /// Loads the name and url of the item image in the first item box of the given wiki page url asynchronously.
        /// </summary>
        /// <param name="urlSuffix">The non-host part of the wiki url that should be loaded.</param>
        /// <returns>The url of the item image.</returns>
        public async Task<string> LoadItemBoxImageAsync(string urlSuffix)
        {
            var doc = new HtmlDocument();
            var file = await _httpClient.GetStringAsync(WikiUrlPrefix + urlSuffix).ConfigureAwait(false);
            doc.LoadHtml(file);
            var nodes =
                doc.DocumentNode.SelectNodes("//span[contains(@class, 'item-box')]/a[contains(@class, 'image')]/img");
            var node = nodes[0];
            return node.GetAttributeValue("src", "");
        }

        /// <summary>
        /// Parses a item info box.
        /// </summary>
        /// <param name="itemBox">A <see cref="HtmlNode"/> with the item-box class.</param>
        /// <returns>The information contained in the item info box</returns>
        public static WikiItemBox ParseItemBox(HtmlNode itemBox)
        {
            var wikiItemBox = new WikiItemBox();
            var header = itemBox.SelectSingleNode("*[contains(@class, 'header')]");
            if (header.GetAttributeValue("class", "").Contains("-double"))
            {
                wikiItemBox.NameLine = WebUtility.HtmlDecode(header.FirstChild.InnerText);
                wikiItemBox.TypeLine = WebUtility.HtmlDecode(header.LastChild.InnerText);
            }
            else
            {
                wikiItemBox.TypeLine = WebUtility.HtmlDecode(header.InnerText);
            }

            var statGroups = new List<WikiItemStat[]>();
            foreach (var statGroup in itemBox.SelectNodes("*[contains(@class, 'item-stats')]/*[contains(@class, 'group')]"))
            {
                var stats = new List<WikiItemStat>();

                var groupColor = WikiStatColor.Default;
                if (statGroup.GetAttributeValue("class", "").Contains("-value"))
                    groupColor = WikiStatColor.Value;
                else if (statGroup.GetAttributeValue("class", "").Contains("-mod"))
                    groupColor = WikiStatColor.Mod;

                var currentStats = new List<Tuple<string, WikiStatColor>>();
                foreach (var childNode in statGroup.ChildNodes)
                {
                    if (childNode.Name == "br" && currentStats.Any())
                    {
                        stats.Add(new WikiItemStat(currentStats));
                        currentStats.Clear();
                        continue;
                    }

                    var color = groupColor;
                    if (childNode.GetAttributeValue("class", "").Contains("-value"))
                        color = WikiStatColor.Value;
                    else if (childNode.GetAttributeValue("class", "").Contains("-mod"))
                        color = WikiStatColor.Mod;
                    else if (childNode.GetAttributeValue("class", "").Contains("-default"))
                        color = WikiStatColor.Default;
                    var text = WebUtility.HtmlDecode(childNode.InnerText);

                    var last = currentStats.LastOrDefault();
                    if (last != null && last.Item2 == color)
                        currentStats[currentStats.Count - 1] = Tuple.Create(last.Item1 + text, color);
                    else
                        currentStats.Add(Tuple.Create(text, color));
                }

                if (currentStats.Any())
                {
                    stats.Add(new WikiItemStat(currentStats));
                }
                statGroups.Add(stats.ToArray());
            }
            wikiItemBox.StatGroups = statGroups.ToArray();
            return wikiItemBox;
        }


        /// <summary>
        /// Tuple of a <see cref="HtmlNode"/> and a <see cref="Model.Items.Enums.ItemType"/> instance
        /// </summary>
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

    public class WikiItemBox
    {
        /// <summary>
        /// Gets the text shown on the first line of the item box.
        /// May be null, e.g. for base items, gems and magic items.
        /// </summary>
        public string NameLine { get; internal set; }

        /// <summary>
        /// Gets the text shown on the second line of the item box.
        /// </summary>
        public string TypeLine { get; internal set; }

        /// <summary>
        /// Gets the stats shown in the item box. First index selects the group (each group is
        /// separated), second index the stat line in that group.
        /// </summary>
        public WikiItemStat[][] StatGroups { get; internal set; }
    }

    /// <summary>
    /// Specifies one stat line in a item info box.
    /// </summary>
    public class WikiItemStat
    {
        /// <summary>
        /// Gets the parts of text of this line with their corresponding colors.
        /// Two successive tuples always have different colors.
        /// </summary>
        public Tuple<string, WikiStatColor>[] Stats { get; }

        /// <summary>
        /// Gets the text of this line.
        /// </summary>
        public string StatsCombined { get; }

        internal WikiItemStat(IEnumerable<Tuple<string, WikiStatColor>> stats)
        {
            Stats = stats.ToArray();
            StatsCombined = string.Join("", Stats.Select(t => t.Item1));
        }
    }

    /// <summary>
    /// Specifies the color with which a part of text is shown.
    /// </summary>
    public enum WikiStatColor
    {
        /// <summary>
        /// Default text color.
        /// </summary>
        Default,
        /// <summary>
        /// Text color for values.
        /// </summary>
        Value,
        /// <summary>
        /// Text color for modifiers. Normally a whole group has this color.
        /// If values surrounded by <see cref="Default"/> colored text have this color,
        /// they are affected by modifiers.
        /// </summary>
        Mod
    }
}