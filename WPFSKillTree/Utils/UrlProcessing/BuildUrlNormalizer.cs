using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using POESKillTree.Localization;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Converts redirects and short links into direct builders and planners links.
    /// </summary>
    public class BuildUrlNormalizer
    {
        private readonly Regex _prefixRegex = new Regex(@"(?<toReplace>^(http(s|):\/\/|)(www\.|)(?<hostname>.*?))(\/|\?|#)");
        private readonly Regex _poeUrlRegex = new Regex(@"(http(s|):\/\/)(www\.|)poeurl\.com\/");

        private readonly Dictionary<string, string> _urlCompletionMap = new Dictionary<string, string>
        {
            // If supports both http and https - prefer secured
            { "goo.gl", "https://goo.gl" },
            { "poeurl.com", "http://poeurl.com" },
            { "tinyurl.com", "https://tinyurl.com" },
            { "poeplanner.com", "http://poeplanner.com" },
            { "pathofexile.com", "https://pathofexile.com" }
        };

        /// <summary>
        /// Creates link to official pathofexile.com builder.
        /// Extracts build url from google link if needed, resolves shortened urls and removes unused query parameters.
        /// </summary>
        public virtual async Task<string> NormalizeAsync(string buildUrl, Func<string, Task, Task> loadingWrapper = null)
        {
            buildUrl = Regex.Replace(buildUrl, @"\s", "");

            while (true)
            {
                if (buildUrl.Contains("google.com"))
                {
                    buildUrl = ExtractUrlFromQuery(buildUrl, "q");
                    continue;
                }

                if (buildUrl.Contains("tinyurl.com") || buildUrl.Contains("poeurl.com") || buildUrl.Contains("goo.gl"))
                {
                    buildUrl = await ResolveShortenedUrl(buildUrl, loadingWrapper);
                    continue;
                }

                break;
            }

            return EnsureProtocol(buildUrl);
        }

        protected virtual string ExtractUrlFromQuery(string url, string parameterName)
        {
            var match = Regex.Match(url, $@"{parameterName}=(?<urlParam>.*?)(&|$)");
            if (!match.Success)
                throw new ArgumentException($"The URL doesn't contain required query parameter '{parameterName}'."); // internal exception

            return match.Groups["urlParam"].Value;
        }

        protected virtual async Task<string> ResolveShortenedUrl(string buildUrl, Func<string, Task, Task> loadingWrapper)
        {
            buildUrl = EnsureProtocol(buildUrl);

            bool allowForbidden = false;

            if (_poeUrlRegex.IsMatch(buildUrl))
            {
                // Fix for a bug in poeplanner: it returns 403 when getting 88+ points, but content is valid
                allowForbidden = true;

                buildUrl = buildUrl.Replace("preview.", "");
                if (!buildUrl.Contains("redirect.php"))
                {
                    buildUrl = _poeUrlRegex.Replace(buildUrl, "http://www.poeurl.com/redirect.php?url=");
                }
            }

            HttpResponseMessage response = null;

            if (loadingWrapper != null)
            {
                // This lambda is used to skip GetAsync() result value in loadingWrapper parameter
                // and thereby unify it by using Task instead of Task<HttpResponseMessage> in signature
                Func<Task> headersLoader = async () =>
                {
                    response = await new HttpClient().GetAsync(buildUrl, HttpCompletionOption.ResponseHeadersRead);
                };

                await loadingWrapper(L10n.Message("Resolving shortened tree address"), headersLoader());

                if (!allowForbidden)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            else
            {
                response = await new HttpClient().GetAsync(buildUrl, HttpCompletionOption.ResponseHeadersRead);
            }

            return response.RequestMessage.RequestUri.ToString();
        }

        protected virtual string EnsureProtocol(string buildUrl)
        {
            var match = _prefixRegex.Match(buildUrl);

            if (!match.Success)
                return buildUrl;

            string completion;
            if (_urlCompletionMap.TryGetValue(match.Groups["hostname"].Value, out completion))
            {
                buildUrl = buildUrl.Replace(match.Groups["toReplace"].Value, completion);
            }

            return buildUrl;
        }
    }
}