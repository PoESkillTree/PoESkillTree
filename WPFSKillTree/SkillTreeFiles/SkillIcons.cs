using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillIcons
    {
        public enum IconType
        {
            Normal,
            Notable,
            Keystone
        }

        public static string urlpath = "http://www.pathofexile.com/image/build-gen/passive-skill-sprite/";
        public Dictionary<string, BitmapImage> Images = new Dictionary<string, BitmapImage>();

        public Dictionary<string, KeyValuePair<Rect, string>> SkillPositions =
            new Dictionary<string, KeyValuePair<Rect, string>>();

        public void OpenOrDownloadImages(SkillTree.UpdateLoadingWindow update = null)
        {
            //Application
            foreach (string image in Images.Keys.ToArray())
            {
                if (!File.Exists("Data\\Assets\\" + image))
                {
                    var _WebClient = new WebClient();
                    _WebClient.DownloadFile(urlpath + image, "Data\\Assets\\" + image);
                }
                Images[image] = ImageHelper.OnLoadBitmapImage(new Uri("Data\\Assets\\" + image, UriKind.Relative));
            }
        }
    }
}