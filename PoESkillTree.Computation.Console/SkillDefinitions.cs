using System.Collections.Generic;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Console
{
    public static class SkillDefinitions
    {
        public static readonly IReadOnlyList<SkillDefinition> Skills = new[]
        {
            CreateSkill("SummonSkeletons", "Summon Skeleton", 0, new Keyword[0], false),
            CreateSkill("VaalSummonSkeletons", "Vaal Summon Skeletons", 1, new Keyword[0], false),
            CreateSkill("RaiseSpectre", "Raise Spectre", 2, new Keyword[0], false),
            CreateSkill("RaiseZombie", "Raise Zombie", 3, new Keyword[0], false),
            CreateSkill("GemDetonateMines", "Detonate Mines", 4, new Keyword[0], false),
            CreateSkill("FrostBlades", "Frost Blades", 5, new Keyword[0], false),
            CreateSkill("SummonIceGolem", "Summon Ice Golem", 6, new Keyword[0], true),
            CreateSkill("SummonFireGolem", "Summon Flame Golem", 7, new Keyword[0], true),
            CreateSkill("SummonLightningGolem", "Summon Lightning Golem", 8, new Keyword[0], true),
            CreateSkill("Convocation", "Convocation", 9, new Keyword[0], false),
            CreateSkill("BlinkArrow", "Blink Arrow", 10, new Keyword[0], false),
            CreateSkill("MirrorArrow", "Mirror Arrow", 11, new Keyword[0], false),
            CreateSkill("HeraldOfIce", "Herald of Ice", 12, new[] { Keyword.Herald }, true),
            CreateSkill("HeraldOfAsh", "Herald of Ash", 13, new[] { Keyword.Herald }, true),
            CreateSkill("HeraldOfThunder", "Herald of Thunder", 14, new[] { Keyword.Herald }, true),
            CreateSkill("HeraldOfPurity", "Herald of Purity", 15, new[] { Keyword.Herald }, true),
        };

        private static SkillDefinition CreateSkill(
            string id, string displayName, int numericId, IReadOnlyList<Keyword> keywords, bool providesBuff)
            => SkillDefinition.CreateActive(
                id, numericId, "", null,
                new ActiveSkillDefinition(displayName, 0, new string[0], new string[0], keywords, providesBuff,
                    null, new ItemClass[0]),
                new Dictionary<int, SkillLevelDefinition>());
    }
}