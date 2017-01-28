using System.Threading.Tasks;
using System.Windows.Media;
using log4net;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class ItemImage : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImage));

        private const string OfficialSiteUrl = "https://www.pathofexile.com";

        private readonly ItemImageService _itemImageService;

        private NotifyingTask<ImageSource> _imageSource;
        public NotifyingTask<ImageSource> ImageSource
        {
            get { return _imageSource; }
            private set { SetProperty(ref _imageSource, value); }
        }

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
            // todo use AsyncEx NotifyTaskCompletion instead
            ImageSource = new NotifyingTask<ImageSource>(task, e => Log.Error(errorMessage, e))
            {
                Default = defaultValue
            };
        }

        private static string MakeUrl(string imageUrl)
        {
            if (imageUrl.StartsWith("/"))
            {
                return OfficialSiteUrl + imageUrl;
            }
            return imageUrl;
        }
    }
}