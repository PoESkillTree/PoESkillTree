using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.StatTranslation
{
    public class StatTranslationLoader
    {
        public const string MainFileName = "stat_translations";
        public const string SkillFileName = MainFileName + "/skill";
        public const string CustomFileName = MainFileName + "/custom";

        public static readonly IReadOnlyList<string> RePoETranslationFileNames = new[]
        {
            MainFileName,
            SkillFileName,
            "stat_translations/support_gem",
            "stat_translations/aura_skill",
            "stat_translations/beam_skill",
            "stat_translations/curse_skill",
            "stat_translations/debuff_skill",
            "stat_translations/minion_attack_skill",
            "stat_translations/minion_skill",
            "stat_translations/minion_spell_skill",
            "stat_translations/offering_skill",
        };

        private readonly IDictionary<string, Task<StatTranslator>> _loadTasks =
            new Dictionary<string, Task<StatTranslator>>();

        private StatTranslationLoader()
        {
        }

        public static Task<StatTranslationLoader> CreateAsync()
            => CreateAsync(RePoETranslationFileNames.Append(CustomFileName));

        private static async Task<StatTranslationLoader> CreateAsync(IEnumerable<string> translationFileNames)
        {
            var loader = new StatTranslationLoader();
            var tasks = translationFileNames.Select(loader.LoadAsync);
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return loader;
        }

        public StatTranslator this[string translationFileName]
            => _loadTasks[translationFileName].GetAwaiter().GetResult();

        private async Task<StatTranslator> LoadAsync(string translationFileName)
            => await _loadTasks.GetOrAdd(translationFileName, StaticLoadAsync).ConfigureAwait(false);

        public static async Task<StatTranslator> StaticLoadAsync(string translationFileName)
        {
            var statTranslations = await DataUtils.LoadRePoEAsync<List<JsonStatTranslation>>(translationFileName)
                .ConfigureAwait(false);
            return new StatTranslator(statTranslations);
        }
    }
}