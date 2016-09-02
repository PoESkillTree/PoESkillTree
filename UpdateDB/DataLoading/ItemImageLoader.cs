using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Loads the images of all base items from the unofficial Wiki at Gamepedia.
    /// </summary>
    public class ItemImageLoader : MultiDataLoader<Task<byte[]>>
    {
        private readonly bool _overwriteExisting;

        private HttpClient _httpClient;

        private WikiUtils _wikiUtils;

        public ItemImageLoader(bool overwriteExisting)
        {
            _overwriteExisting = overwriteExisting;
        }

        protected override async Task LoadAsync(HttpClient httpClient)
        {
            if (Directory.Exists(SavePath))
                Directory.Delete(SavePath, true);
            Directory.CreateDirectory(SavePath);

            _httpClient = httpClient;
            _wikiUtils = new WikiUtils(httpClient);
            var jewelTask = Task.WhenAll(ItemGroup.Jewel.Types().Select(LoadJewelAsync));
            await _wikiUtils.ForEachBaseItemAsync(ParseTable);
            await jewelTask;
        }

        private async Task LoadJewelAsync(ItemType jewel)
        {
            var jewelName = jewel.ToString();
            var url = await _wikiUtils.LoadItemBoxImageAsync(jewelName.Replace("Jewel", "_Jewel"));
            SaveImage(jewelName.Replace("Jewel", " Jewel"), url);
        }

        private void ParseTable(HtmlNode table, ItemType itemType)
        {
            // Go through the first cell for each row
            foreach (var cell in table.SelectNodes("tr/td[1]/span"))
            {
                var imgNode = cell.SelectSingleNode("a/img");
                var url = Regex.Match(imgNode.Attributes["srcset"].Value, @"1\.5x, (.*) 2x").Groups[1].Value;
                var fileName = WebUtility.HtmlDecode(cell.SelectNodes("a")[0].InnerHtml) + ".png";
                SaveImage(fileName, url);
            }
        }

        private void SaveImage(string fileName, string url)
        {
            if (_overwriteExisting || !File.Exists(Path.Combine(SavePath, fileName)))
                AddSaveTask(fileName, _httpClient.GetByteArrayAsync(url));
        }

        protected override async Task SaveDataToStreamAsync(Task<byte[]> data, Stream stream)
        {
            using (var ms = new MemoryStream(await data))
            using (var image = Image.FromStream(ms))
            {
                var resized = image.Resize((int)(image.Width * WikiUtils.ItemImageResizeFactor),
                    (int)(image.Height * WikiUtils.ItemImageResizeFactor));
                resized.Save(stream, ImageFormat.Png);
            }
        }
    }
}