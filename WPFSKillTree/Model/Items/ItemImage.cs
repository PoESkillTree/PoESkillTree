using System.Threading.Tasks;
using System.Windows.Media;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    /// <summary>
    /// Represents an asynchronously loaded image for an item group or item base or from an url stored in an item's
    /// json object.
    /// </summary>
    public class ItemImage : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImage));

        private const string OfficialSiteUrl = "https://www.pathofexile.com";

        private readonly ItemImageService _itemImageService;

        private NotifyingTask<ImageSource> _imageSource;
        /// <summary>
        /// Gets a <see cref="NotifyingTask{TResult}"/> for the represented image.
        /// </summary>
        public NotifyingTask<ImageSource> ImageSource
        {
            get { return _imageSource; }
            private set { SetProperty(ref _imageSource, value); }
        }

        /// <summary>
        /// Represents an image for an item group. The image will be loaded synchronously.
        /// </summary>
        public ItemImage(ItemImageService itemImageService, ItemGroup baseGroup)
        {
            _itemImageService = itemImageService;
            var defaultImage = itemImageService.LoadDefaultImage(baseGroup);
            NewImageSourceTask(
                Task.FromResult(defaultImage),
                "Exception in completed task",
                defaultImage
            );
        }

        /// <summary>
        /// Represents an image for an item base. First the group's image will be loaded synchronously,
        /// which is then used as the image until the base item's image is loaded asynchronously.
        /// </summary>
        public ItemImage(ItemImageService itemImageService, string baseName, ItemGroup baseGroup)
        {
            _itemImageService = itemImageService;
            var defaultImage = itemImageService.LoadDefaultImage(baseGroup);
            NewImageSourceTask(
                itemImageService.LoadItemImageAsync(baseName, defaultImage),
                "Loading of base item image failed",
                defaultImage
            );
        }

        /// <summary>
        /// Represents an image that is loaded asynchronously from an url. Only urls stored in an item's json as
        /// retrieved from the official api are supported.
        /// </summary>
        public ItemImage(ItemImage baseItemImage, string imageUrl)
        {
            _itemImageService = baseItemImage._itemImageService;
            _imageSource = baseItemImage._imageSource;
            NewImageSourceTask(
                _itemImageService.LoadFromUrl(MakeUrl(imageUrl), ImageSource.Result),
                "Downloading of item image from official url failed.",
                ImageSource.Result
            );
        }

        private void NewImageSourceTask(Task<ImageSource> task, string errorMessage, ImageSource defaultValue)
        {
            ImageSource = new NotifyingTask<ImageSource>(task, e => Log.Error(errorMessage, e))
            {
                Default = defaultValue
            };
        }

        private static string MakeUrl(string imageUrl)
        {
            // if the image's url has no domain, the domain is the official site
            if (imageUrl.StartsWith("/"))
            {
                return OfficialSiteUrl + imageUrl;
            }
            return imageUrl;
        }
    }
}