using System.Collections.Generic;
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

namespace UpdateEquipment.DataLoading
{
    public class ItemImageLoader : MultiDataLoader<Task<byte[]>>
    {
        private static readonly IEnumerable<string> JewelUrls = ItemGroup.Jewel.Types()
            .Select(t => t.ToString().Replace("Jewel", "_Jewel")).ToList();

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
            var jewelTask = Task.WhenAll(JewelUrls.Select(LoadJewelAsync));
            await _wikiUtils.ForEachBaseItemAsync(ParseTable);
            await jewelTask;
        }

        private async Task LoadJewelAsync(string url)
        {
            var tuple = await _wikiUtils.LoadItemBoxImageAsync(url);
            SaveImage(tuple.Item1, tuple.Item2.ToString());
        }

        private void ParseTable(HtmlNode table, ItemType itemType)
        {
            foreach (var row in table.Elements("tr").Skip(1))
            {
                var cell = row.ChildNodes[0];
                var imgNode = cell.SelectSingleNode("a/img");
                var url = Regex.Match(imgNode.Attributes["srcset"].Value, @"1\.5x, (.*) 2x").Groups[1].Value;
                var fileName = WebUtility.HtmlDecode(cell.GetAttributeValue("data-sort-value", "")) + ".png";
                SaveImage(fileName, url);
            }
        }

        private void SaveImage(string fileName, string url)
        {
            if (_overwriteExisting || !File.Exists(Path.Combine(SavePath, fileName)))
                Save(fileName, _httpClient.GetByteArrayAsync(url));
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