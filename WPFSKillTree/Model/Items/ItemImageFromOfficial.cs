using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using log4net;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class ItemImageFromOfficial : ItemImage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImageFromOfficial));

        private static readonly string PathFormat =
            Path.Combine(AppData.GetFolder(), "Data", "Equipment", "Downloaded", "{0}");

        private const string OfficialSiteUrl = "https://www.pathofexile.com";

        public ItemImageFromOfficial(ItemImage baseImage, string imageUrl)
            : base(baseImage)
        {
            NewImageSourceTask(LoadFromOfficial(MakeUrl(imageUrl)),
                "Downloading of item image from official url failed.", ImageSource.Result);
        }

        private static async Task<ImageSource> LoadFromOfficial(string imageUrl)
        {
            // Remove the query part, remove the host part, remove image/Art/2DItems/, trim slashes
            var relevantPart = Regex.Replace(imageUrl, @"(\?.*)|(.*(\.net|\.com)/)|(image/Art/2DItems/)", "").Trim('/');
            var match = Regex.Match(relevantPart, @"gen/image/.*?/([a-zA-Z0-9]*)/Item\.png");
            if (match.Success)
            {
                // These names are too long.
                // They contain groups of 20 chars (as folders) and end with a unique identifier.
                relevantPart = string.Format("gen/{0}.png", match.Groups[1]);
            }
            var fileName = string.Format(PathFormat, relevantPart);
            if (!File.Exists(fileName))
                await DownloadFromOfficial(fileName, imageUrl).ConfigureAwait(false);
            return await Task.Run(() => ImageSourceFromPath(fileName)).ConfigureAwait(false);
        }

        private static async Task DownloadFromOfficial(string fileName, string imageUrl)
        {
            using (var client = new HttpClient())
            {
                var imgData = await client.GetByteArrayAsync(imageUrl).ConfigureAwait(false);
                CreateDirectories(fileName);
                using (var ms = new MemoryStream(imgData))
                using (var image = Image.FromStream(ms))
                {
                    try
                    {
                        using (var outputStream = File.Create(fileName, 65536, FileOptions.Asynchronous))
                        {
                            image.Save(outputStream, ImageFormat.Png);
                        }
                        Log.InfoFormat("Downloaded item image {0} to the file system.", fileName);
                        return;
                    }
                    catch (IOException e)
                    {
                        Log.Info("File " + fileName +
                                 " could not be created. Most likely because it was created from another thread.", e);
                        Log.Info("Waiting 1 sec for the other thread to finish.");
                    }
                    // No await in catch
                    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                }
            }
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