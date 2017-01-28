using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils.WikiApi
{
    public class ApiAccessor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApiAccessor));

        private const string BaseUri = "https://pathofexile.gamepedia.com/api.php?format=json&formatversion=2";

        private readonly HttpClient _httpClient;

        public ApiAccessor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<JToken>> AskArgs(IEnumerable<string> conditions, IEnumerable<string> printouts)
        {
            var queryString = new StringBuilder();
            queryString.Append("&action=askargs");
            queryString.Append("&conditions=");
            queryString.Append(string.Join("|", conditions));
            queryString.Append("&printouts=");
            queryString.Append(string.Join("|", printouts));
            queryString.Append("&parameters=limit=200");
            return await AskApi(queryString.ToString());
        }

        public async Task<IEnumerable<JToken>> Ask(IEnumerable<string> conditions, IEnumerable<string> printous)
        {
            var queryString = new StringBuilder();
            queryString.Append("&action=ask");
            queryString.Append("&query=");
            conditions.Select(s => $"[[{s}]]").ForEach(s => queryString.Append(s));
            printous.Select(s => $"|?{s}").ForEach(s => queryString.Append(s));
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

        public async Task<IEnumerable<Tuple<string, string>>> QueryImageInfoUrls(IEnumerable<string> titles)
        {
            const int maxTitlesPerRequest = 50;
            var batches = titles
                .Select((t, i) => new { Index = i, Title = t })
                .GroupBy(x => x.Index / maxTitlesPerRequest)
                .Select(g => g.Select(x => x.Title))
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

        private static bool LogErrors(JObject json, string uri)
        {
            JToken errorToken;
            if (json.TryGetValue("error", out errorToken))
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
            JToken warningsToken;
            if (json.TryGetValue("warnings", out warningsToken))
            {
                var warnings = warningsToken as JContainer;
                if (warnings != null)
                {
                    Log.Warn($"Api returned warnings for uri {uri}:");
                    // e.g. warnings.main.warnings.Value is the path to the first warning string
                    warnings.SelectMany(t => t).SelectMany(t => t).Cast<JProperty>().Select(p => p.Value.Value<string>()).ForEach(Log.Warn);
                }
            }
        }
    }
}