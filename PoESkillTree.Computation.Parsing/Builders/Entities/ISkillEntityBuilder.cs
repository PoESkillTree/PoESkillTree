using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    public interface ISkillEntityBuilder : IEntityBuilder
    {
        // Limits the targets this instance describes
        ISkillEntityBuilder With(IKeywordBuilder keyword);
        ISkillEntityBuilder With(params IKeywordBuilder[] keywords);
        ISkillEntityBuilder From(ISkillBuilder skill);
    }
}