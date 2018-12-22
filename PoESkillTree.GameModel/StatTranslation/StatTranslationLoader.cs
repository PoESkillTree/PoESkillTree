using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.StatTranslation
{
    public class StatTranslationLoader
    {
        public const string MainFileName = "stat_translations";
        public const string SkillFileName = MainFileName + "/skill";
        public const string CustomFileName = "custom_stat_translations.json";

        public static readonly IReadOnlyList<string> RePoETranslationFileNames = new[]
        {
            MainFileName,
            SkillFileName,
            "stat_translations/support_gem",
            "stat_translations/aura_skill",
            "stat_translations/banner_aura_skill",
            "stat_translations/beam_skill",
            "stat_translations/brand_skill",
            "stat_translations/curse_skill",
            "stat_translations/debuff_skill",
            "stat_translations/minion_attack_skill",
            "stat_translations/minion_skill",
            "stat_translations/minion_spell_skill",
            "stat_translations/offering_skill",
            "stat_translations/variable_duration_skill",
        };

        private readonly IReadOnlyDictionary<string, StatTranslator> _translators;

        private StatTranslationLoader(IReadOnlyDictionary<string, StatTranslator> translators)
            => _translators = translators;

        public static async Task<StatTranslationLoader> CreateAsync()
        {
            var fileNames = RePoETranslationFileNames.Append(CustomFileName).ToList();
            var tasks = fileNames.Select(LoadAsync);
            var translators = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dict = fileNames.EquiZip(translators, (f, t) => (f, t)).ToDictionary();
            return new StatTranslationLoader(dict);
        }

        public StatTranslator this[string translationFileName] => _translators[translationFileName];

        public static async Task<StatTranslator> LoadAsync(string translationFileName)
        {
            var loadTask = translationFileName == CustomFileName
                ? DataUtils.LoadJsonAsync<List<JsonStatTranslation>>(translationFileName)
                : DataUtils.LoadRePoEAsync<List<JsonStatTranslation>>(translationFileName);
            var statTranslations = await loadTask.ConfigureAwait(false);
            return new StatTranslator(statTranslations);
        }
    }
}