using System.Threading.Tasks;
using POESKillTree.SkillTreeFiles;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Loads the skill tree assets using <see cref="AssetLoader"/>.
    /// </summary>
    public class SkillTreeLoader : DataLoader
    {
        public override bool SavePathIsFolder
        {
            get { return false; }
        }

        protected override async Task LoadAsync()
        {
            var assetLoader = new AssetLoader(HttpClient, SavePath, true);
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