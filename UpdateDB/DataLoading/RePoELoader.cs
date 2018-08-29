using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.GameModel;

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
            "mods", "crafting_bench_options", "npc_master", "stat_translations", "default_monster_stats", "characters",
            "gems", "gem_tooltips", "stat_translations/skill",
        };

        public override bool SavePathIsFolder => true;

        protected override async Task LoadAsync()
        {
            Directory.CreateDirectory(Path.Combine(SavePath, "stat_translations"));
            await Task.WhenAll(Files.Select(LoadAsync));
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