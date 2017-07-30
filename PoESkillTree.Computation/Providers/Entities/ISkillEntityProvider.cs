using PoESkillTree.Computation.Providers.Skills;

namespace PoESkillTree.Computation.Providers.Entities
{
    public interface ISkillEntityProvider : IEntityProvider
    {
        // Limits the targets this instance describes
        ISkillEntityProvider With(IKeywordProvider keyword);
        ISkillEntityProvider With(params IKeywordProvider[] keywords);
        ISkillEntityProvider From(ISkillProvider skill);
    }
}