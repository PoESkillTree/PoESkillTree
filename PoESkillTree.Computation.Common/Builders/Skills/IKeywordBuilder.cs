using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Skills
{
    /// <summary>
    /// Represents a keyword describing skills, e.g. Attack or Projectile. A skill's keywords are retrieved from
    /// its ActiveSkillTypes and gem tags.
    /// </summary>
    public interface IKeywordBuilder : IResolvable<IKeywordBuilder>
    {

    }
}