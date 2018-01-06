using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;
using static POESKillTree.Utils.WikiApi.WikiApiUtils;

namespace POESKillTree.Utils.WikiApi
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
        public async Task<IEnumerable<JToken>> CargoQuery(
            IEnumerable<string> tables, IEnumerable<string> fields, string where, string joinOn = "")
        {
            var uri = BuildCargoQueryUri(tables, fields, where, joinOn);
            try
            {
                var json = JObject.Parse(await _httpClient.GetStringAsync(uri).ConfigureAwait(false));
                LogWarnings(json, uri);
                if (!LogErrors(json, uri))
                {
                    return json["cargoquery"].Select(j => j["title"]).ToList();
                }
                return Enumerable.Empty<JToken>();
            }
            catch (JsonException e)
            {
                Log.Error($"Retrieving cargoquery results from {uri} failed", e);
                return Enumerable.Empty<JToken>();
            }
        }

        private static string BuildCargoQueryUri(
            IEnumerable<string> tables, IEnumerable<string> fields, string where, string joinOn)
        {
            var queryString = new StringBuilder()
                .Append("&action=cargoquery")
                .Append("&limit=500")
                .Append("&tables=").Append(string.Join(",", tables))
                .Append("&fields=").Append(string.Join(",", fields.Select(s => s.Replace(' ', '_'))))
                .Append("&where=").Append(where)
                .Append("&join_on=").Append(joinOn);
            return BaseUri + queryString;
        }

        /// <summary>
        /// Queries the API using the args action.
        /// </summary>
        /// <param name="conditions">the query conditions. A retrieved subject must satisfy all of them.</param>
        /// <param name="printouts">the query printouts. These are the RDF properties retrieved for each subject.
        /// </param>
        /// <returns>A task that returns an enumerable of the printouts of all subjects matching the conditions.
        /// </returns>
        /// <remarks>
        /// Only supports short queries (combination of conditions and printouts). Does support disjunctions in
        /// conditions.
        /// </remarks>
        public async Task<IEnumerable<JToken>> Ask(IEnumerable<string> conditions, IEnumerable<string> printouts)
        {
            var queryString = new StringBuilder();
            queryString.Append("&action=ask");
            queryString.Append("&query=");
            conditions.Select(s => $"[[{s}]]").ForEach(s => queryString.Append(s));
            printouts.Select(s => $"|?{s}").ForEach(s => queryString.Append(s));
            queryString.Append("|limit=200");
            return await AskApi(queryString.ToString());
        }

        private async Task<IEnumerable<JToken>> AskApi(string queryString)
        {
            var initialUri = BaseUri + queryString;
            var uri = initialUri;
            try
            {
                var results = new List<JToken>();
                while (true)
                {
                    var json = JObject.Parse(await _httpClient.GetStringAsync(uri).ConfigureAwait(false));
                    if (!LogErrors(json, uri))
                    {
                        results.AddRange(json["query"]["results"].Cast<JProperty>().Select(p => p.Value["printouts"]));
                    }
                    LogWarnings(json, uri);

                    JToken offsetToken;
                    if (!json.TryGetValue("query-continue-offset", out offsetToken))
                        break;
                    var offset = offsetToken.Value<int>();
                    uri = initialUri + "|offset=" + offset;
                }
                return results;
            }
            catch (JsonException e)
            {
                Log.Error($"Retrieving askargs results from {uri} failed", e);
                return Enumerable.Empty<JProperty>();
            }
        }

        /// <summary>
        /// Queries the API using the 'query' action, 'imageinfo' prop and 'url' iiprop.
        /// </summary>
        /// <param name="titles">the titles for which to query the imageinfor.url property</param>
        /// <returns>
        /// A task that returns an enumerable of tuples of titles and their imageinfo.url property.
        /// </returns>
        public async Task<IEnumerable<Tuple<string, string>>> QueryImageInfoUrls(IEnumerable<string> titles)
        {
            const int maxTitlesPerRequest = 50;
            var batches = titles
                .Batch(maxTitlesPerRequest)
                .Select(QueryImageInfoUrlsBatch)
                .ToList();

            var results = new List<Tuple<string, string>>();
            foreach (var batch in batches)
            {
                results.AddRange(await batch.ConfigureAwait(false));
            }
            return results;
        }

        private async Task<IEnumerable<Tuple<string, string>>> QueryImageInfoUrlsBatch(IEnumerable<string> titles)
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
                        select Tuple.Create(title, url);
                }
                LogWarnings(json, uri);
            }
            catch (JsonException e)
            {
                Log.Error($"Retrieving query-imageinfo-url results from {uri} failed", e);
            }
            return Enumerable.Empty<Tuple<string, string>>();
        }

        private static bool HasImageInfo(JToken imageInfoEntry, string title)
        {
            if (imageInfoEntry.Value<bool>("missing"))
            {
                Log.Warn("Missing image for " + title);
                return false;
            }
            if (imageInfoEntry["imageinfo"] == null)
            {
                Log.Error("No imageinfo entry for " + title);
                return false;
            }
            return true;
        }

        /// <summary>
        /// First queries the API using the 'ask' action, the given conditions and RdfName and RdfIcon printouts.
        /// The queries the API using the 'query' action, 'imageinfo' prop and 'url' iiprop with the icon page titles
        /// from the first query.
        /// </summary>
        /// <param name="conditions">the conditions items have to much to have their icon urls retrieved</param>
        /// <returns>
        /// A task that returns an enumerable of ItemImageInfoResults.
        /// </returns>
        public async Task<IEnumerable<ItemImageInfoResult>> AskAndQueryImageInforUrls(
            IEnumerable<string> conditions)
        {
            // Ask: retrieve page titles of the icons
            string[] printouts = { RdfName, RdfIcon };
            var results = (from ps in await Ask(conditions, printouts).ConfigureAwait(false)
                           where ps[RdfIcon].Any()
                           let title = ps[RdfIcon].First.Value<string>("fulltext")
                           let name = SingularValue<string>(ps, RdfName)
                           select new { name, title }).ToList();
            var titleToName = results.ToLookup(x => x.title, x => x.name);

            // QueryImageInfoUrls: retrieve urls of the icons
            var task = QueryImageInfoUrls(results.Select(t => t.title));
            return
                from tuple in await task.ConfigureAwait(false)
                select new ItemImageInfoResult(titleToName[tuple.Item1], tuple.Item2);
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