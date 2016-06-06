using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Utils;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillIcons
    {
        public const int NormalIconWidth = 27;
        public const int NotableIconWidth = 38;
        public const int KeystoneIconWidth = 53;
        public const int MasteryIconWidth = 99;

        private const string Urlpath = "http://www.pathofexile.com/image/build-gen/passive-skill-sprite/";

        public readonly Dictionary<string, BitmapImage> Images = new Dictionary<string, BitmapImage>();

        public readonly Dictionary<string, KeyValuePair<Rect, string>> SkillPositions =
            new Dictionary<string, KeyValuePair<Rect, string>>();

        public async Task OpenOrDownloadImages(HttpClient httpClient, [CanBeNull] ProgressDialogController controller,
            double progressRange)
        {
            var perImageProgress = progressRange / Images.Count;
            foreach (var image in Images.Keys.ToArray())
            {
                var path = SkillTree.AssetsFolderPath + image;
                if (!File.Exists(path))
                {
                    var stream = await httpClient.GetStreamAsync(Urlpath + image);
                    await FileEx.WriteStreamAsync(path, stream);
                }
                Images[image] =
                    ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));
                if (controller != null)
                    controller.IncreaseProgress(perImageProgress);
            }
        }
    }
}