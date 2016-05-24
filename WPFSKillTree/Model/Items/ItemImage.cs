using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Items
{
    public class ItemImage : Notifier
    {
        private static readonly string FilePathFormat = Path.Combine(AppData.GetFolder(), "Data", "Equipment", "Assets", "{0}.png");

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImage));

        private static readonly Dictionary<ItemGroup, ImageSource> DefaultImageCache =
            new Dictionary<ItemGroup, ImageSource>();

        private static readonly ImageSource ErrorImage = Imaging.CreateBitmapSourceFromHIcon(
            SystemIcons.Error.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

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

        public ItemImage(string baseName, ItemGroup baseGroup)
        {
            _baseName = baseName;
            _baseGroup = baseGroup;
            _defaultImage = LoadDefaultImage();
            LoadImage();
        }

        public void DownloadMissingImage()
        {
            // todo Don't do this if disabled in settings
            if (!_isDefaultImage)
                return;
            ImageSource = new NotifyingTask<ImageSource>(LoadFromWiki(), e =>
            {
                Log.Error("Downloading of missing base item image failed", e);
            })
            {
                Default = _defaultImage
            };
        }

        private void LoadImage()
        {
            var fileName = string.Format(FilePathFormat, _baseName);
            if (File.Exists(fileName))
            {
                _isDefaultImage = false;
                ImageSource = new NotifyingTask<ImageSource>(Task.Run(() => ImageSourceFromPath(fileName)), e =>
                {
                    Log.Error("Loading of base item image failed", e);
                })
                {
                    Default = _defaultImage
                };
            }
            else
            {
                ImageSource = new NotifyingTask<ImageSource>(Task.FromResult(_defaultImage));
            }
        }

        private ImageSource LoadDefaultImage()
        {
            if (!DefaultImageCache.ContainsKey(_baseGroup))
            {
                try
                {
                    var path = "pack://application:,,,/PoESkillTree;component/Images/EquipmentUI/ItemDefaults/" + _baseGroup + ".png";
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

        private async Task<ImageSource> LoadFromWiki()
        {
            using (var client = new HttpClient())
            {
                var wikiUtils = new WikiUtils(client);
                var imgTuple = await wikiUtils.LoadItemBoxImageAsync(_baseName).ConfigureAwait(false);
                var imgData = await client.GetByteArrayAsync(imgTuple.Item2).ConfigureAwait(false);
                var fileName = string.Format(FilePathFormat, _baseName);
                using (var ms = new MemoryStream(imgData))
                using (var image = Image.FromStream(ms))
                using (var outputStream = File.Create(fileName, 65536, FileOptions.Asynchronous))
                {
                    var resized = image.Resize((int)(image.Width * WikiUtils.ItemImageResizeFactor),
                        (int)(image.Height * WikiUtils.ItemImageResizeFactor));
                    resized.Save(outputStream, ImageFormat.Png);
                }
                Log.InfoFormat("Downloaded base item image for {0} to the file system.", _baseName);
                return ImageSourceFromPath(fileName);
            }
        }

    }
}