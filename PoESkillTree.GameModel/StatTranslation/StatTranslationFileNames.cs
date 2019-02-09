using System.Collections.Generic;

namespace PoESkillTree.GameModel.StatTranslation
{
    public static class StatTranslationFileNames
    {
        public const string Main = "stat_translations";
        public const string Skill = Main + "/skill";
        public const string Custom = "custom_stat_translations.json";

        public static readonly IReadOnlyList<string> AllFromRePoE = new[]
        {
            Main, Skill,
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
    }
}