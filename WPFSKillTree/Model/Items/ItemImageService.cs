using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.Utils.WikiApi;

namespace POESKillTree.Model.Items
{
    public class ItemImageService
    {
        // todo don't do much if there's no internet

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImageService));

        private static readonly string AssetPath =
            Path.Combine(AppData.GetFolder(), "Data", "Equipment", "Assets");
        private static readonly string AssetPathFormat = 
            Path.Combine(AssetPath, "{0}.png");

        private static readonly string DownloadedPathFormat =
            Path.Combine(AppData.GetFolder(), "Data", "Equipment", "Downloaded", "{0}");

        private const string ResourcePathFormat = 
            "pack://application:,,,/PoESkillTree;component/Images/EquipmentUI/ItemDefaults/{0}.png";

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly PoolingImageLoader _wikiLoader;
        private readonly Options _options;

        private readonly ConcurrentDictionary<ItemGroup, ImageSource> _defaultImageCache = 
            new ConcurrentDictionary<ItemGroup, ImageSource>();

        private readonly ConcurrentDictionary<string, Task<ImageSource>> _assetImageCache = 
            new ConcurrentDictionary<string, Task<ImageSource>>();

        private readonly ConcurrentDictionary<string, Task<ImageSource>> _downloadedImageCache =
            new ConcurrentDictionary<string, Task<ImageSource>>();

        private readonly ImageSource _errorImage = Imaging.CreateBitmapSourceFromHIcon(
            SystemIcons.Error.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions()
        );

        public ItemImageService(Options options)
        {
            _options = options;
            _wikiLoader = new PoolingImageLoader(_httpClient);
            Directory.CreateDirectory(AssetPath);
        }

        public ImageSource LoadDefaultImage(ItemGroup group)
        {
            return _defaultImageCache.GetOrAdd(group, g =>
            {
                if (Application.Current == null)
                    return _errorImage;
                try
                {
                    var path = string.Format(ResourcePathFormat, g);
                    return ImageSourceFromPath(path);
                }
                catch (Exception e)
                {
                    Log.Warn("Could not load default file for ItemGroup " + g, e);
                    return _errorImage;
                }
            });
        }

        public Task<ImageSource> LoadItemImageAsync(string itemName, ImageSource defaultImage)
        {
            return _assetImageCache.GetOrAdd(itemName, n => LoadAssetAsync(itemName, defaultImage));
        }

        private async Task<ImageSource> LoadAssetAsync(string itemName, ImageSource defaultImage)
        {
            var fileName = string.Format(AssetPathFormat, itemName);
            if (File.Exists(fileName))
            {
                return await Task.Run(() => ImageSourceFromPath(fileName)).ConfigureAwait(false);
            }
            if (!_options.DownloadMissingItemImages)
            {
                return defaultImage;
            }
            await _wikiLoader.ProduceAsync(itemName, fileName).ConfigureAwait(false);
            return ImageSourceFromPath(fileName);
        }

        public Task<ImageSource> LoadFromUrl(string imageUrl, ImageSource defaultImage)
        {
            return _downloadedImageCache.GetOrAdd(imageUrl, n => LoadOfficialAsync(n, defaultImage));
        }

        private async Task<ImageSource> LoadOfficialAsync(string imageUrl, ImageSource defaultImage)
        {
            // Remove the query part, remove the host part, remove image/Art/2DItems/, trim slashes
            var relevantPart = Regex.Replace(imageUrl, @"(\?.*)|(.*(\.net|\.com)/)|(image/Art/2DItems/)", "").Trim('/');
            var match = Regex.Match(relevantPart, @"gen/image/.*?/([a-zA-Z0-9]*)/Item\.png");
            if (match.Success)
            {
                // These names are too long.
                // They contain groups of 20 chars (as folders) and end with a unique identifier.
                relevantPart = $"gen/{match.Groups[1]}.png";
            }
            var fileName = string.Format(DownloadedPathFormat, relevantPart);

            if (!File.Exists(fileName))
            {
                if (!_options.DownloadMissingItemImages)
                {
                    return defaultImage;
                }
                var imgData = await _httpClient.GetByteArrayAsync(imageUrl).ConfigureAwait(false);
                CreateDirectories(fileName);
                using (var outputStream = File.Create(fileName))
                {
                    WikiApiUtils.SaveImage(imgData, outputStream, false);
                }
                Log.Info($"Downloaded item image {fileName} to the file system.");
            }
            return await Task.Run(() => ImageSourceFromPath(fileName)).ConfigureAwait(false);
        }

        private static ImageSource ImageSourceFromPath(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            img.EndInit();
            img.Freeze();
            return img;
        }

        private static void CreateDirectories(string fileName)
        {
            var f = new FileInfo(fileName);
            if (f.DirectoryName != null)
            {
                Directory.CreateDirectory(f.DirectoryName);
            }
        }
    }
}