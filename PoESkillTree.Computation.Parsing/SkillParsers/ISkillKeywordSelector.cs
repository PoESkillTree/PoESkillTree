using System.Collections.Generic;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public interface ISkillKeywordSelector
    {
        IEnumerable<Keyword> GetKeywords(SkillDefinition skillDefinition);

        IReadOnlyList<IEnumerable<Keyword>> GetKeywordsPerPart(SkillDefinition skillDefinition);
    }

    public class ActiveSkillKeywordSelector : ISkillKeywordSelector
    {
        public IEnumerable<Keyword> GetKeywords(SkillDefinition skillDefinition)
            => skillDefinition.ActiveSkill.Keywords;

        public IReadOnlyList<IEnumerable<Keyword>> GetKeywordsPerPart(SkillDefinition skillDefinition)
            => skillDefinition.ActiveSkill.KeywordsPerPart;
    }

    public class SupportSkillKeywordSelector : ISkillKeywordSelector
    {
        public IEnumerable<Keyword> GetKeywords(SkillDefinition skillDefinition)
            => skillDefinition.SupportSkill.AddedKeywords;

        public IReadOnlyList<IEnumerable<Keyword>> GetKeywordsPerPart(SkillDefinition skillDefinition)
            => new[] { GetKeywords(skillDefinition) };
    }
}