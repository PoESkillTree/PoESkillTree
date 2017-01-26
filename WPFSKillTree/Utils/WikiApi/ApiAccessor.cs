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
            var initialUriBuilder = new StringBuilder(BaseUri);
            initialUriBuilder.Append("&action=askargs");
            initialUriBuilder.Append("&conditions=");
            initialUriBuilder.Append(string.Join("|", conditions));
            initialUriBuilder.Append("&printouts=");
            initialUriBuilder.Append(string.Join("|", printouts));
            initialUriBuilder.Append("&parameters=limit=200");
            var initialUri = initialUriBuilder.ToString();

            var uri = initialUri;
            try
            {
                var results = new List<JToken>();
                while (true)
                {
                    var json = JObject.Parse(await _httpClient.GetStringAsync(uri));
                    if (!LogErrors(json, uri))
                    {
                        results.AddRange(json["query"]["results"]);
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
                return Enumerable.Empty<JToken>();
            }
        }

        public async Task<string> QueryImageInfoUrl(string title)
        {
            var results = await QueryImageInfoUrlsChunk(new[] {title});
            return results.FirstOrDefault()?.Item2;
        }

        public async Task<IEnumerable<Tuple<string, string>>> QueryImageInfoUrls(IEnumerable<string> titles)
        {
            const int maxTitlesPerRequest = 50;
            var chunks = titles
                .Select((t, i) => new { Index = i, Title = t })
                .GroupBy(x => x.Index / maxTitlesPerRequest)
                .Select(g => g.Select(x => x.Title))
                .Select(QueryImageInfoUrlsChunk)
                .ToList();

            var results = new List<Tuple<string, string>>();
            foreach (var chunk in chunks)
            {
                results.AddRange(await chunk);
            }
            return results;
        }

        private async Task<IEnumerable<Tuple<string, string>>> QueryImageInfoUrlsChunk(IEnumerable<string> titles)
        {
            var uriBuilder = new StringBuilder(BaseUri);
            uriBuilder.Append("&action=query&prop=imageinfo&iiprop=url");
            uriBuilder.Append("&titles=");
            uriBuilder.Append(string.Join("|", titles));
            var uri = uriBuilder.ToString();

            try
            {
                var json = JObject.Parse(await _httpClient.GetStringAsync(uri));
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
                Log.Error($"Api returned an error with code {errorToken.Value<string>("code")} for uri {uri}:");
                Log.Error(errorToken.Value<string>("info"));
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
                    warnings.Descendants().Where(t => !t.Any()).ForEach(Log.Warn);
                }
            }
        }
    }
}