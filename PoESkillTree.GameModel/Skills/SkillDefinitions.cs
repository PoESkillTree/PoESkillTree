using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinitions
    {
        private readonly Lazy<IReadOnlyDictionary<string, SkillDefinition>> _skillDict;

        public SkillDefinitions(IReadOnlyList<SkillDefinition> skills)
        {
            Skills = skills;
            _skillDict = new Lazy<IReadOnlyDictionary<string, SkillDefinition>>(
                () => Skills.ToDictionary(s => s.Id));
        }

        public IReadOnlyList<SkillDefinition> Skills { get; }

        public SkillDefinition GetSkillById(string skillId) => _skillDict.Value[skillId];
    }
}