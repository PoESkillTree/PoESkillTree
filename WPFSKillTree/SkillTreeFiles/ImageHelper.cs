using System;
using System.Windows.Media.Imaging;

namespace POESKillTree.SkillTreeFiles
{
    public class ImageHelper
    {
        static public BitmapImage OnLoadBitmapImage(Uri uri)
        {
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = uri;
            bim.EndInit();
            return bim;
        }
    }
}
