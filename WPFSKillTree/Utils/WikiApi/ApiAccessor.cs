using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.Utils.WikiApi
{
    /// <summary>
    /// Provides access to the wiki's API.
    /// </summary>
    public class ApiAccessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApiAccessor));

        private const string BaseUri = "https://pathofexile.gamepedia.com/api.php?format=json&formatversion=2";

        private readonly HttpClient _httpClient;

        public ApiAccessor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Queries the API using the cargoquery action.
        /// </summary>
        public async Task<IReadOnlyList<JToken>> CargoQueryAsync(
            IReadOnlyList<string> tables, IEnumerable<string> fields, string where, string joinOn = "")
        {
            var results = new List<JToken>();
            List<JToken> newResults;
            var baseUri = BuildCargoQueryUri(tables, fields, where, joinOn) + "&offset=";
            int limit;
            do
            {
                var json = await CargoQueryAsync(baseUri + results.Count).ConfigureAwait(false);
                if (json is null)
                    return new JToken[0];

                newResults = json["cargoquery"].Select(j => j["title"]).ToList();
                limit = json["limits"].Value<int>("cargoquery");
                results.AddRange(newResults);
            } while (newResults.Count >= limit);
            return results;
        }

        private static string BuildCargoQueryUri(
            IReadOnlyList<string> tables, IEnumerable<string> fields, string where, string joinOn)
        {
            var allFields = fields
                .Union(tables.Select(GetPageNameField))
                .Select(s => s.Replace(' ', '_'));
            var queryString = new StringBuilder()
                .Append("&action=cargoquery")
                .Append("&limit=max")
                .Append("&tables=").Append(string.Join(",", tables))
                .Append("&fields=").Append(string.Join(",", allFields))
                .Append("&where=").Append(AddValidPageNameConditionToWhereClause(where, tables))
                .Append("&join_on=").Append(joinOn)
                .Append("&order_by=").Append(GetPageNameFieldAlias(tables.First()));
            return BaseUri + queryString;
        }

        [ItemCanBeNull]
        private async Task<JObject> CargoQueryAsync(string uri)
        {
            try
            {
                Log.Debug($"Getting {uri} ...");
                var json = JObject.Parse(await _httpClient.GetStringAsync(uri).ConfigureAwait(false));
                LogWarnings(json, uri);
                return LogErrors(json, uri) ? null : json;
            }
            catch (JsonException e)
            {
                Log.Error($"Retrieving cargoquery results from {uri} failed", e);
                return null;
            }
        }

        private static string AddValidPageNameConditionToWhereClause(string where, IEnumerable<string> tables)
        {
            var validPageNameCondition = tables.Select(t => $"{t}.{CargoConstants.PageName} NOT LIKE 'User:%'");
            return $"({where}) AND {string.Join(" AND ", validPageNameCondition)}";
        }

        private static string GetPageNameField(string table)
            => $"{table}.{CargoConstants.PageName}={GetPageNameFieldAlias(table)}";

        public static string GetPageNameFieldAlias(string table)
            => $"{table}_page_name";

        /// <summary>
        /// Queries the API using the 'query' action, 'imageinfo' prop and 'url' iiprop.
        /// </summary>
        /// <param name="titles">the titles for which to query the imageinfor.url property</param>
        /// <returns>
        /// A task that returns an enumerable of tuples of titles and their imageinfo.url property.
        /// </returns>
        private async Task<IEnumerable<(string title, string url)>> QueryImageInfoUrlsAsync(IEnumerable<string> titles)
        {
            const int maxTitlesPerRequest = 40;
            var batches = titles.Batch(maxTitlesPerRequest, QueryImageInfoUrlsBatchAsync);

            var results = new List<(string title, string url)>();
            foreach (var batch in batches)
            {
                results.AddRange(await batch.ConfigureAwait(false));
            }
            return results;
        }

        private async Task<IEnumerable<(string title, string url)>> QueryImageInfoUrlsBatchAsync(IEnumerable<string> titles)
        {
            var uriBuilder = new StringBuilder(BaseUri);
            uriBuilder.Append("&action=query&prop=imageinfo&iiprop=url");
            uriBuilder.Append("&titles=");
            uriBuilder.Append(string.Join("|", titles));
            var uri = uriBuilder.ToString();

            try
            {
                var json = JObject.Parse(await _httpClient.GetStringAsync(uri).ConfigureAwait(false));
                if (!LogErrors(json, uri))
                {
                    return
                        from result in json["query"]["pages"]
                        let title = result.Value<string>("title")
                        where HasImageInfo(result, title)
                        let url = result["imageinfo"].First.Value<string>("url")
                        select (title, url);
                }
                LogWarnings(json, uri);
            }
            catch (JsonException e)
            {
                Log.Error($"Retrieving query-imageinfo-url results from {uri} failed", e);
            }
            catch(HttpRequestException ex)
            {
                Log.Error($"Retrieving query-imageinfo-url results from {uri} failed", ex);
            }
            return Enumerable.Empty<(string, string)>();
        }

        private static bool HasImageInfo(JToken imageInfoEntry, string title)
        {
            if (imageInfoEntry["imageinfo"] == null)
            {
                Log.Error("No imageinfo entry for " + title);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asynchronously returns <see cref="ItemImageInfoResult"/> for all items matching the given condition.
        /// </summary>
        public async Task<IEnumerable<ItemImageInfoResult>> GetItemImageInfosAsync(string where)
        {
            // CargoQuery: retrieve page titles of the icons
            string[] tables = { CargoConstants.ItemTableName };
            string[] fields = { CargoConstants.Name, CargoConstants.InventoryIcon };
            var results = (
                from cargoResult in await CargoQueryAsync(tables, fields, @where).ConfigureAwait(false)
                let name = cargoResult.Value<string>(CargoConstants.Name)
                let iconPageTitle = cargoResult.Value<string>(CargoConstants.InventoryIcon)
                select new { name, iconPageTitle }
            ).DistinctBy(x => x.iconPageTitle).ToList();
            var titleToName = results.ToLookup(x => x.iconPageTitle, x => x.name);

            // QueryImageInfoUrls: retrieve urls of the icons
            var task = QueryImageInfoUrlsAsync(results.Select(t => t.iconPageTitle));
            return
                from tuple in await task.ConfigureAwait(false)
                select new ItemImageInfoResult(titleToName[tuple.title], tuple.url);
        }

        private static bool LogErrors(JObject json, string uri)
        {
            if (json.TryGetValue("error", out var errorToken))
            {
                var code = errorToken.Value<string>("code");
                if (code != null)
                {
                    Log.Error($"Api returned an error with code {errorToken.Value<string>("code")} for uri {uri}:");
                    Log.Error(errorToken.Value<string>("info"));
                    return true;
                }
                Log.Error($"Api returned errors for uri {uri}:");
                errorToken.SelectMany(t => t).SelectMany(t => t).Select(t => t.Value<string>()).ForEach(Log.Error);
                return true;
            }
            return false;
        }

        private static void LogWarnings(JObject json, string uri)
        {
            if (json.TryGetValue("warnings", out var warningsToken))
            {
                if (warningsToken is JContainer warnings)
                {
                    Log.Warn($"Api returned warnings for uri {uri}:");
                    // e.g. warnings.main.warnings.Value is the path to the first warning string
                    warnings.SelectMany(t => t).SelectMany(t => t).Cast<JProperty>().Select(p => p.Value.Value<string>()).ForEach(Log.Warn);
                }
            }
        }


        // Some items have the same image, e.g. Agnerod North/East/South/West
        public struct ItemImageInfoResult
        {
            /// <summary>
            /// Gets the names of the items.
            /// </summary>
            public IEnumerable<string> Names { get; }
            /// <summary>
            /// Gets the url of the image for the named items.
            /// </summary>
            public string Url { get; }

            public ItemImageInfoResult(IEnumerable<string> names, string url)
            {
                Names = names;
                Url = url;
            }
        }
    }
}