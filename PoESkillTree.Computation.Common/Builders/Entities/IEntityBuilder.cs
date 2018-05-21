using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Entities
{
    /// <summary>
    /// Represents an entity that is source and target of modifier applications, can be affected by effect, 
    /// can be source and target of actions and similar.
    /// </summary>
    public interface IEntityBuilder : IResolvable<IEntityBuilder>
    {
        /// <summary>
        /// Returns <paramref name="stat"/> from the context of this entity instead of the default Self.
        /// </summary>
        IDamageStatBuilder Stat(IDamageStatBuilder stat);

        /// <summary>
        /// Returns <paramref name="stat"/> from the context of this entity instead of the default Self.
        /// </summary>
        IFlagStatBuilder Stat(IFlagStatBuilder stat);

        /// <summary>
        /// Returns <paramref name="stat"/> from the context of this entity instead of the default Self.
        /// </summary>
        IPoolStatBuilder Stat(IPoolStatBuilder stat);

        /// <summary>
        /// Returns <paramref name="stat"/> from the context of this entity instead of the default Self.
        /// </summary>
        IStatBuilder Stat(IStatBuilder stat);
    }
}