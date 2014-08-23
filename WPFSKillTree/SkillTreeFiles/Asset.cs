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

        public Asset(string name, string url)
        {
            Name = name;
            URL = url;
            if (!File.Exists("Data\\Assets\\" + Name + ".png"))
            {
                var webClient = new WebClient();
                webClient.DownloadFile(URL, "Data\\Assets\\" + Name + ".png");
            }
            PImage = new BitmapImage(new Uri("Data\\Assets\\" + Name + ".png", UriKind.Relative));
        }
    }
}