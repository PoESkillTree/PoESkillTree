using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;

namespace UpdateDB.DataLoading
{
    // todo skill tree data loading is too wired into the SkillTree class to be configurable here
    // (it should be properly async, use the provided httpClient and not instantiate an actual SkillTree instance)
    /// <summary>
    /// Loads the skill tree assets using <see cref="SkillTree"/>.
    /// </summary>
    public class SkillTreeLoader : DataLoader
    {
        public override bool SavePathIsFolder
        {
            get { return false; }
        }

        protected override Task LoadAsync(HttpClient httpClient)
        {
            return Task.Run(async () =>
            {
                // SkillTree does nothing if the files are already there.
                // Data/Assets
                var assetsFolder = Path.Combine(SavePath, "Assets");
                if (Directory.Exists(assetsFolder))
                    Directory.Delete(assetsFolder, true);
                Directory.CreateDirectory(assetsFolder);
                // Data/Opts.txt
                var optsFile = Path.Combine(SavePath, "Opts.txt");
                if (File.Exists(optsFile))
                    File.Delete(optsFile);
                // Data/Skilltree.txt
                var treeFile = Path.Combine(SavePath, "Skilltree.txt");
                if (File.Exists(treeFile))
                    File.Delete(treeFile);

                await SkillTree.CreateAsync(new PersistentData(false), null);
            });
        }

        protected override Task CompleteSavingAsync()
        {
            return Task.WhenAll();
        }
    }
}