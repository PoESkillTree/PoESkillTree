using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Console
{
    public static class SkillDefinitions
    {
        public static readonly IReadOnlyList<SkillDefinition> Skills = new[]
        {
            new SkillDefinition("Summon Skeleton", 0, new Keyword[0], false),
            new SkillDefinition("Vaal Summon Skeletons", 1, new Keyword[0], false),
            new SkillDefinition("Raise Spectre", 2, new Keyword[0], false),
            new SkillDefinition("Raise Zombie", 3, new Keyword[0], false),
            new SkillDefinition("Detonate Mines", 4, new Keyword[0], false),
            new SkillDefinition("Frost Blades", 5, new Keyword[0], false),
            new SkillDefinition("Ice Golem", 6, new Keyword[0], true),
            new SkillDefinition("Flame Golem", 7, new Keyword[0], true),
            new SkillDefinition("Lightning Golem", 8, new Keyword[0], true),
            new SkillDefinition("Convocation", 9, new Keyword[0], false),
            new SkillDefinition("Blink Arrow", 10, new Keyword[0], false),
            new SkillDefinition("Mirror Arrow", 11, new Keyword[0], false),
            new SkillDefinition("Herald of Ice", 12, new[] { Keyword.Herald }, true),
            new SkillDefinition("Herald of Ash", 13, new[] { Keyword.Herald }, true),
            new SkillDefinition("Herald of Thunder", 14, new[] { Keyword.Herald }, true),
        };

        public static readonly IReadOnlyList<string> SkillNames = Skills.Select(s => s.DisplayName).ToList();
    }
}