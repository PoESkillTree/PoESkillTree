using System.Collections.Generic;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinition
    {
        public SkillDefinition(string skillName, int numericId, IReadOnlyList<Keyword> keywords, bool providesBuff)
        {
            Id = skillName;
            DisplayName = skillName;
            NumericId = numericId;
            Keywords = keywords;
            ProvidesBuff = providesBuff;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int NumericId { get; }
        public IReadOnlyList<Keyword> Keywords { get; }
        public bool ProvidesBuff { get; }
    }
}