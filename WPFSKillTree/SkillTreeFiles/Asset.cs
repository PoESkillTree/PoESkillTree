using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using POESKillTree.Utils;

namespace POESKillTree.SkillTreeFiles
{
    public class Asset
    {
        public BitmapImage PImage { get; private set; }

        private Asset()
        {
            
        }

        private async Task InitializeAsync(HttpClient httpClient, string name, string url)
        {
            var path = SkillTree.AssetsFolderPath + name + ".png";
            if (!File.Exists(path))
            {
                var stream = await httpClient.GetStreamAsync(url);
                await FileEx.WriteStreamAsync(path, stream);
            }
            PImage = ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));

        }

        public static async Task<Asset> CreateAsync(HttpClient httpClient, string name, string url)
        {
            var asset = new Asset();
            await asset.InitializeAsync(httpClient, name, url);
            return asset;
        }
    }
}