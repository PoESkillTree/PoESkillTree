using System.Threading.Tasks;
using System.Windows.Media;
using NLog;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.Items
{
    /// <summary>
    /// Represents an asynchronously loaded image for an item group or item base or from an url stored in an item's
    /// json object.
    /// </summary>
    public class ItemImage : Notifier
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private const string OfficialSiteUrl = "https://www.pathofexile.com";

        /// <summary>
        /// Gets a <see cref="NotifyingTask{TResult}"/> for the represented image.
        /// </summary>
        public NotifyingTask<ImageSource> ImageSource { get; }

        /// <summary>
        /// Represents an image for an item class. The image will be loaded synchronously.
        /// </summary>
        public ItemImage(ItemImageService itemImageService, ItemClass baseClass)
        {
            var defaultImage = itemImageService.LoadDefaultImage(baseClass);
            ImageSource = NewImageSourceTask(
                Task.FromResult(defaultImage),
                "Exception in completed task",
                defaultImage
            );
        }

        /// <summary>
        /// Represents an image for an item base. First the classes's image will be loaded synchronously,
        /// which is then used as the image until the base item's image is loaded asynchronously.
        /// </summary>
        public ItemImage(ItemImageService itemImageService, string baseName, ItemClass baseClass)
        {
            var defaultImage = itemImageService.LoadDefaultImage(baseClass);
            ImageSource = NewImageSourceTask(
                itemImageService.LoadItemImageAsync(baseName, Task.FromResult(defaultImage)),
                "Loading of base item image failed",
                defaultImage
            );
        }

        private ItemImage(ImageSource defaultImage, Task<ImageSource> imageTask, string errorMessage)
        {
            ImageSource = NewImageSourceTask(imageTask, errorMessage, defaultImage);
        }

        /// <summary>
        /// Returns an image that is loaded asynchronously from an url. Only urls stored in an item's json as
        /// retrieved from the official api are supported. This image is used as default as long as
        /// the loading is not done and in case it fails.
        /// </summary>
        public ItemImage AsDefaultForImageFromUrl(ItemImageService itemImageService, string imageUrl)
        {
            return new ItemImage(
                ImageSource.Result,
                itemImageService.LoadFromUrlAsync(MakeUrl(imageUrl), ImageSource.Task),
                "Downloading of item image from official url failed"
            );
        }

        /// <summary>
        /// Returns a unique image that is loaded asynchronously.
        /// This image is used as default as long as the loading is not done and in case it fails.
        /// </summary>
        public ItemImage AsDefaultForUniqueImage(ItemImageService itemImageService, string uniqueName)
        {
            return new ItemImage(
                ImageSource.Result,
                itemImageService.LoadItemImageAsync(uniqueName, ImageSource.Task),
                "Loading of unique item image failed"
            );
        }

        private static NotifyingTask<ImageSource> NewImageSourceTask(Task<ImageSource> task, string errorMessage,
            ImageSource defaultValue)
        {
            return new NotifyingTask<ImageSource>(task, e => Log.Error(e, errorMessage))
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