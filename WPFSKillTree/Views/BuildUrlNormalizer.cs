using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Views
{
    /// <summary>
    /// Converts redirects and short links into direct builders and planners links.
    /// </summary>
    public class BuildUrlNormalizer
    {
        private readonly Regex _protoRegex = new Regex(@"(?<toReplace>^(http(s|):\/\/|)(www\.|)(?<hostname>.*?))(\/|\?)");

        private readonly Dictionary<string, string> _hostsCompletionMap = new Dictionary<string, string>
        {
            // If supports both - prefer secured
            { "goo.gl", "https://www.goo.gl" },
            { "poeurl.com", "http://www.poeurl.com" },
            { "tinyurl.com", "https://www.tinyurl.com" }
        };

        /// <summary>
        /// Creates link to official pathofexile.com builer.
        /// Extracts build url from google link if needed, resolves shortened urls and removes unused query parameters.
        /// </summary>
        public virtual async Task<string> NormalizeAsync(string treeUrl, Func<string, Task, Task> loadingWrapper)
        {
            while (true)
            {
                if (treeUrl.Contains("google.com"))
                {
                    treeUrl = ExtractUrlFromQuery(treeUrl);
                    continue;
                }

                if (treeUrl.Contains("tinyurl.com") || treeUrl.Contains("poeurl.com") || treeUrl.Contains("goo.gl"))
                {
                    treeUrl = await ResolveShortenedUrl(treeUrl, loadingWrapper);
                    continue;
                }

                break;
            }

            // Remove all query parameters
            treeUrl = Regex.Replace(treeUrl, @"\?.*", "");

            // Replace scheme, authority and path to manualy redirect to pathofexile.com
            treeUrl = Regex.Replace(treeUrl, Constants.TreeRegex, Constants.TreeAddress);

            return treeUrl;
        }

        private string ExtractUrlFromQuery(string treeUrl)
        {
            var match = Regex.Match(treeUrl, @"q=(?<urlParam>.*?)(&|$)");
            if (!match.Success)
                throw new Exception("The URL you are trying to load is invalid.");

            return match.Groups["urlParam"].Value;
        }

        private async Task<string> ResolveShortenedUrl(string treeUrl, Func<string, Task, Task> loadingWrapper)
        {
            treeUrl = EnsureProtocol(treeUrl);

            var skillUrl = treeUrl.Replace("preview.", "");
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

        private string EnsureProtocol(string treeUrl)
        {
            var match = _protoRegex.Match(treeUrl);

            if (!match.Success)
                return treeUrl;

            string completion;
            if (_hostsCompletionMap.TryGetValue(match.Groups["hostname"].Value, out completion))
            {
                treeUrl = treeUrl.Replace(match.Groups["toReplace"].Value, completion);
            }

            return treeUrl;
        }
    }
}