using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace POESKillTree.SkillTreeFiles
{
    public class Asset
    {
        public string Name;
        public BitmapImage PImage;
        public string URL;

        public Asset(string name, string url, string dataFolder)
        {
            Name = name;
            URL = url;
            if (!File.Exists( dataFolder + "\\Assets\\" + Name + ".png"))
            {
                var webClient = new WebClient();
                webClient.DownloadFile(URL, dataFolder + "\\Assets\\" + Name + ".png");
            }

            PImage = ImageHelper.OnLoadBitmapImage(new Uri(dataFolder + "\\Assets\\" + Name + ".png", UriKind.Relative));
        }
    }
}