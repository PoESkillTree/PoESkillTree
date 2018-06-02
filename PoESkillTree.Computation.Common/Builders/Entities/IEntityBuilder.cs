using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Entities
{
    /// <summary>
    /// Represents an entity that is source and target of modifier applications, can be affected by effect, 
    /// can be source and target of actions and similar.
    /// </summary>
    public interface IEntityBuilder : IResolvable<IEntityBuilder>
    {
        /// <summary>
        /// Builds to a collection of entities. The collection is empty if this entity doesn't restrict the entity,
        /// i.e. <see cref="IEntityBuilders.Self"/>.
        /// </summary>
        IReadOnlyCollection<Entity> Build(Entity modifierSourceEntity);
    }
}