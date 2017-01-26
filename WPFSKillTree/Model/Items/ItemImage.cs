using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    public class ItemImage : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImage));

        private static readonly string AssetPathFormat =
            Path.Combine(AppData.GetFolder(), "Data", "Equipment", "Assets", "{0}.png");

        private const string ResourcePathFormat =
            "pack://application:,,,/PoESkillTree;component/Images/EquipmentUI/ItemDefaults/{0}.png";

        private static readonly Dictionary<ItemGroup, ImageSource> DefaultImageCache =
            new Dictionary<ItemGroup, ImageSource>();

        private static readonly ImageSource ErrorImage = Imaging.CreateBitmapSourceFromHIcon(
            SystemIcons.Error.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        protected Options Options { get; }

        private readonly string _baseName;
        private readonly ItemGroup _baseGroup;

        private NotifyingTask<ImageSource> _imageSource;
        public NotifyingTask<ImageSource> ImageSource
        {
            get { return _imageSource; }
            private set { SetProperty(ref _imageSource, value); }
        }

        private bool _isDefaultImage = true;
        private readonly ImageSource _defaultImage;

        public ItemImage(Options options, string baseName, ItemGroup baseGroup)
        {
            _baseName = baseName;
            _baseGroup = baseGroup;
            Options = options;
            _defaultImage = LoadDefaultImage();
            LoadImage();
        }

        protected ItemImage(ItemImage baseItemImage)
        {
            Options = baseItemImage.Options;
            _baseName = baseItemImage._baseName;
            _baseGroup = baseItemImage._baseGroup;
            _imageSource = baseItemImage._imageSource;
            _isDefaultImage = baseItemImage._isDefaultImage;
            _defaultImage = baseItemImage._defaultImage;
        }

        public void DownloadMissingImage()
        {
            if (!_isDefaultImage || !Options.DownloadMissingItemImages)
                return;
            NewImageSourceTask(LoadFromWiki(), "Downloading of missing base item image failed", _defaultImage);
        }

        private void LoadImage()
        {
            var fileName = string.Format(AssetPathFormat, _baseName);
            if (File.Exists(fileName))
            {
                _isDefaultImage = false;
                NewImageSourceTask(Task.Run(() => ImageSourceFromPath(fileName)), "Loading of base item image failed",
                    _defaultImage);
            }
            else
            {
                ImageSource = new NotifyingTask<ImageSource>(Task.FromResult(_defaultImage));
            }
        }

        private ImageSource LoadDefaultImage()
        {
            // This is only for UnitTests. Don't need item not found warnings there.
            if (Application.Current == null)
                return ErrorImage;

            if (!DefaultImageCache.ContainsKey(_baseGroup))
            {
                try
                {
                    var path = string.Format(ResourcePathFormat, _baseGroup);
                    DefaultImageCache[_baseGroup] = ImageSourceFromPath(path);
                }
                catch (Exception e)
                {
                    Log.Warn("Could not load default file for ItemGroup " + _baseGroup, e);
                    DefaultImageCache[_baseGroup] = ErrorImage;
                }
            }
            return DefaultImageCache[_baseGroup];
        }

        protected static ImageSource ImageSourceFromPath(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            img.EndInit();
            img.Freeze();
            return img;
        }

        private async Task<ImageSource> LoadFromWiki()
        {
            using (var client = new HttpClient())
            {
                var apiAccessor = new ApiAccessor(client);
                var conditions = new ConditionBuilder
                {
                    {ItemRdfPredicates.RdfName, _baseName}
                };
                var results = await apiAccessor.AskArgs(conditions, new[] {ItemRdfPredicates.RdfIcon}).ConfigureAwait(false);
                var iconJson = results.First().First["printouts"][ItemRdfPredicates.RdfIcon].First;
                var title = iconJson.Value<string>("fulltext");
                var url = await apiAccessor.QueryImageInfoUrl(title).ConfigureAwait(false);

                var imgData = await client.GetByteArrayAsync(url).ConfigureAwait(false);
                var fileName = string.Format(AssetPathFormat, _baseName);
                CreateDirectories(fileName);
                using (var outputStream = File.Create(fileName, 65536))
                {
                    WikiApiUtils.ResizeAndSaveImage(imgData, outputStream);
                }
                Log.InfoFormat("Downloaded base item image for {0} to the file system.", _baseName);
                return ImageSourceFromPath(fileName);
            }
        }

        protected void NewImageSourceTask(Task<ImageSource> task, string errorMessage, ImageSource defaultValue)
        {
            ImageSource = new NotifyingTask<ImageSource>(task, e => Log.Error(errorMessage, e))
            {
                Default = defaultValue
            };
        }

        protected static void CreateDirectories(string fileName)
        {
            var f = new FileInfo(fileName);
            if (f.DirectoryName != null)
            {
                Directory.CreateDirectory(f.DirectoryName);
            }
        }
    }
}