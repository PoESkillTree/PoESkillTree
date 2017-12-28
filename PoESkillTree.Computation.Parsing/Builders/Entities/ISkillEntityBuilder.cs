using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    /// <summary>
    /// Represents entities originating from skills, e.g. minions or totems.
    /// </summary>
    public interface ISkillEntityBuilder : IEntityBuilder
    {
        /// <summary>
        /// Returns a new entity that represents only the entities represented by this instance that originate from
        /// skills having the given keyword.
        /// </summary>
        ISkillEntityBuilder With(IKeywordBuilder keyword);

        /// <summary>
        /// Returns a new entity that represents only the entities represented by this instance that originate from
        /// skills having all given keywords.
        /// </summary>
        ISkillEntityBuilder With(params IKeywordBuilder[] keywords);

        /// <summary>
        /// Returns a new entity that represents only the entities represented by this instance that originate from
        /// the given skill.
        /// </summary>
        ISkillEntityBuilder From(ISkillBuilder skill);
    }
}