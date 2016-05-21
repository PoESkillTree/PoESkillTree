using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using POESKillTree.Model.Items;
using UpdateEquipment.Utils;

namespace UpdateEquipment.DataLoading
{
    public class ItemImageLoader : MultiDataLoader<Task<byte[]>>
    {
        private const double ResizeFactor = 0.6;

        private static readonly IEnumerable<string> JewelUrls = ItemGroup.Jewel.Types()
            .Select(t => t.ToString().Replace("Jewel", "_Jewel"))
            .Select(s => Path.Combine(WikiUtils.WikiUrlPrefix, s)).ToList();

        private readonly bool _overwriteExisting;

        private CachedHttpClient _httpClient;

        public ItemImageLoader(bool overwriteExisting)
        {
            _overwriteExisting = overwriteExisting;
        }

        protected override async Task LoadAsync(CachedHttpClient httpClient)
        {
            _httpClient = httpClient;
            var wikiUtils = new WikiUtils(httpClient);
            var jewelTask = Task.WhenAll(JewelUrls.Select(LoadJewelAsync));
            await wikiUtils.ForEachBaseItemAsync(ParseTable);
            await jewelTask;
        }

        private async Task LoadJewelAsync(string url)
        {
            var doc = new HtmlDocument();
            var file = await _httpClient.GetStringAsync(url);
            doc.LoadHtml(file);
            var node = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'itemboximage')]/a/img");
            SaveImage(node.GetAttributeValue("alt", ""), node.GetAttributeValue("src", ""));
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
            {
                using (var image = Image.FromStream(ms))
                {
                    var resized = ResizeImage(image, (int) (image.Width * ResizeFactor),
                        (int) (image.Height * ResizeFactor));
                    resized.Save(stream, ImageFormat.Png);
                }
            }
        }

        // Source: StackOverflow user mpen, http://stackoverflow.com/a/24199315
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}