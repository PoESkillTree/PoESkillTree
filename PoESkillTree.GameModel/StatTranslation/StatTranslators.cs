using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MoreLinq.Extensions.EquiZipExtension;
using static MoreLinq.Extensions.ToDictionaryExtension;

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

        public IStatTranslator this[string translationFileName] => _translators[translationFileName];

        public static async Task<StatTranslators> CreateAsync(bool deserializeOnThreadPool)
        {
            var fileNames = StatTranslationFileNames.AllFromRePoE.Append(StatTranslationFileNames.Custom).ToList();
            var tasks = fileNames.Select(f => CreateAsync(f, deserializeOnThreadPool));
            var translators = await Task.WhenAll(tasks).ConfigureAwait(false);
            var dict = fileNames.EquiZip(translators, (f, t) => (f, t)).ToDictionary();
            return new StatTranslators(dict);
        }

        public static Task<StatTranslator> CreateFromMainFileAsync(bool deserializeOnThreadPool = false)
            => CreateAsync(StatTranslationFileNames.Main, deserializeOnThreadPool);

        private static async Task<StatTranslator> CreateAsync(string translationFileName, bool deserializeOnThreadPool)
        {
            var loadTask = translationFileName == StatTranslationFileNames.Custom
                ? DataUtils.LoadJsonAsync<List<JsonStatTranslation>>(translationFileName, deserializeOnThreadPool)
                : DataUtils.LoadRePoEAsync<List<JsonStatTranslation>>(translationFileName, deserializeOnThreadPool);
            var statTranslations = await loadTask.ConfigureAwait(false);
            return new StatTranslator(statTranslations);
        }
    }
}