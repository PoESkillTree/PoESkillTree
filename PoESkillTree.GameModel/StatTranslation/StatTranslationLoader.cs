using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.StatTranslation
{
    public class StatTranslationLoader
    {
        public static readonly IReadOnlyList<string> TranslationFileNames = new[]
        {
            "stat_translations",
            "stat_translations/skill",
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

        public StatTranslator this[string translationFileName]
            => _loadTasks[translationFileName].Result;

        public async Task<StatTranslator> LoadAsync(string translationFileName)
            => await _loadTasks.GetOrAdd(translationFileName, StaticLoadAsync).ConfigureAwait(false);

        public static async Task<StatTranslator> StaticLoadAsync(string translationFileName = "stat_translations")
        {
            var statTranslations = await DataUtils.LoadRePoEAsync<List<JsonStatTranslation>>(translationFileName)
                .ConfigureAwait(false);
            return new StatTranslator(statTranslations);
        }
    }
}