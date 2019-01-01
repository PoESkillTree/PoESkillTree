using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.StatTranslation;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Downloads json data files from RePoE and saves them to the file system.
    /// </summary>
    public class RePoELoader : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RePoELoader));

        private static readonly string[] Files =
        {
            "mods", "crafting_bench_options", "default_monster_stats", "characters",
            "gems", "gem_tooltips", "base_items"
        };

        public override bool SavePathIsFolder => true;

        protected override async Task LoadAsync()
        {
            Directory.CreateDirectory(Path.Combine(SavePath, "stat_translations"));
            var files = Files.Concat(StatTranslationFileNames.AllFromRePoE);
            await Task.WhenAll(files.Select(LoadAsync));
        }

        private async Task LoadAsync(string file)
        {
            var fileName = file + DataUtils.RePoEFileSuffix;
            var response = await HttpClient.GetAsync(DataUtils.RePoEDataUrl + fileName);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"Failed to load {file}: {response.StatusCode}");
                return;
            }
            using (var writer = File.Create(Path.Combine(SavePath, fileName)))
            {
                await response.Content.CopyToAsync(writer).ConfigureAwait(false);
                Log.Info($"Loaded {file}");
            }
        }
    }
}