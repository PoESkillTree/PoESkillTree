using System;
using System.Windows.Media.Imaging;

namespace PoESkillTree.Utils.Wpf
{
    public static class BitmapImageFactory
    {
        public static BitmapImage Create(string path)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            img.EndInit();
            img.Freeze();
            return img;
        }
    }
}