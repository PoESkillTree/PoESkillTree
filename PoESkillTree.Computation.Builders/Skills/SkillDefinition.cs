using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Skills
{
    public class SkillDefinition
    {
        public SkillDefinition(string skillName, int numericId, IReadOnlyList<Keyword> keywords, bool providesBuff)
        {
            SkillName = skillName;
            NumericId = numericId;
            Keywords = keywords;
            ProvidesBuff = providesBuff;
        }

        public string SkillName { get; }
        public int NumericId { get; }
        public IReadOnlyList<Keyword> Keywords { get; }
        public bool ProvidesBuff { get; }
    }
}