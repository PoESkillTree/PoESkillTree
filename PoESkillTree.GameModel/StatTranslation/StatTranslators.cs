using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Creates and holds <see cref="StatTranslator"/> instances.
    /// </summary>
    public class StatTranslators
    {
        private readonly IReadOnlyDictionary<string, StatTranslator> _translators;

        private StatTranslators(IReadOnlyDictionary<string, StatTranslator> translators)
            => _translators = translators;

        public StatTranslator this[string translationFileName] => _translators[translationFileName];

        public static async Task<StatTranslators> CreateAsync()
        {
            var fileNames = StatTranslationFileNames.AllFromRePoE.Append(StatTranslationFileNames.Custom).ToList();
            var tasks = fileNames.Select(CreateAsync);
            var translators = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dict = fileNames.EquiZip(translators, (f, t) => (f, t)).ToDictionary();
            return new StatTranslators(dict);
        }

        public static Task<StatTranslator> CreateFromMainFileAsync()
            => CreateAsync(StatTranslationFileNames.Main);

        private static async Task<StatTranslator> CreateAsync(string translationFileName)
        {
            var loadTask = translationFileName == StatTranslationFileNames.Custom
                ? DataUtils.LoadJsonAsync<List<JsonStatTranslation>>(translationFileName)
                : DataUtils.LoadRePoEAsync<List<JsonStatTranslation>>(translationFileName);
            var statTranslations = await loadTask.ConfigureAwait(false);
            return new StatTranslator(statTranslations);
        }
    }
}