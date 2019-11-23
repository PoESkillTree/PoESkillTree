using System.Net.Http;
using System.Threading.Tasks;
using PoESkillTree.SkillTreeFiles;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Loads the skill tree assets using <see cref="AssetLoader"/>.
    /// </summary>
    public class SkillTreeLoader : IDataLoader
    {
        public bool SavePathIsFolder => false;

        public async Task LoadAndSaveAsync(HttpClient httpClient, string savePath)
        {
            var assetLoader = new AssetLoader(httpClient, savePath, true);
            try
            {
                await assetLoader.DownloadAllAsync();
                assetLoader.MoveTemp();
            }
            catch
            {
                assetLoader.DeleteTemp();
                throw;
            }
        }
    }
}