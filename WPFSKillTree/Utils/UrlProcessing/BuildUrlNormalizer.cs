using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Converts redirects and short links into direct builders and planners links.
    /// </summary>
    public class BuildUrlNormalizer
    {
        private readonly Regex _prefixRegex = new Regex(@"(?<toReplace>^(http(s|):\/\/|)(www\.|)(?<hostname>.*?))(\/|\?|#)");

        private readonly Dictionary<string, string> _urlCompletionMap = new Dictionary<string, string>
        {
            // If supports both http and https - prefer secured
            { "goo.gl", "https://www.goo.gl" },
            { "poeurl.com", "http://www.poeurl.com" },
            { "tinyurl.com", "https://www.tinyurl.com" },
            { "pathofexile.com", "https://www.pathofexile.com" }
        };

        /// <summary>
        /// Creates link to official pathofexile.com builder.
        /// Extracts build url from google link if needed, resolves shortened urls and removes unused query parameters.
        /// </summary>
        public virtual async Task<string> NormalizeAsync(string buildUrl, Func<string, Task, Task> loadingWrapper)
        {
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
                throw new Exception("The URL you are trying to load is invalid.");

            return match.Groups["urlParam"].Value;
        }

        protected virtual async Task<string> ResolveShortenedUrl(string buildUrl, Func<string, Task, Task> loadingWrapper)
        {
            buildUrl = EnsureProtocol(buildUrl);

            var skillUrl = buildUrl.Replace("preview.", "");
            if (skillUrl.Contains("poeurl.com") && !skillUrl.Contains("redirect.php"))
            {
                skillUrl = skillUrl.Replace("http://www.poeurl.com/", "http://www.poeurl.com/redirect.php?url=");
            }

            HttpResponseMessage response = null;

            // This lambda is used to skip GetAsync() result value in loadingWrapper parameter
            // and thereby unify it by using Task instead of Task<HttpResponseMessage> in signature
            Func<Task> headersLoader = async () =>
            {
                response = await new HttpClient().GetAsync(skillUrl, HttpCompletionOption.ResponseHeadersRead);
            };

            await loadingWrapper(L10n.Message("Resolving shortened tree address"), headersLoader());
            response.EnsureSuccessStatusCode();

            if (!Regex.IsMatch(response.RequestMessage.RequestUri.ToString(), Constants.TreeRegex))
                throw new Exception("The URL you are trying to load is invalid.");

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