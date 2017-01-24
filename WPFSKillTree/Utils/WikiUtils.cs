using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

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

        private readonly HttpClient _httpClient;

        /// <param name="httpClient">The <see cref="HttpClient"/> instance used for internet access.</param>
        public WikiUtils(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
    }
}