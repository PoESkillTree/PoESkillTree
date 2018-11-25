using System.Collections.Generic;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Common.Builders.Entities
{
    /// <summary>
    /// Factory interface for entities.
    /// </summary>
    public interface IEntityBuilders
    {
        /// <summary>
        /// Gets the <see cref="BuildParameters.ModifierSourceEntity"/>.
        /// </summary>
        IEntityBuilder Self { get; }

        /// <summary>
        /// Gets the entity/entities opposing <see cref="BuildParameters.ModifierSourceEntity"/>
        /// (Enemy opposes Character, Minion and Totem).
        /// </summary>
        IEntityBuilder OpponentOfSelf { get; }

        /// <summary>
        /// Gets an entity representing enemies.
        /// </summary>
        IEnemyBuilder Enemy { get; }

        /// <summary>
        /// Gets an entity representing the player character.
        /// </summary>
        IEntityBuilder Character { get; }

        /// <summary>
        /// Gets an entity representing the character's allies.
        /// </summary>
        IEntityBuilder Ally { get; }

        /// <summary>
        /// Gets an entity representing the character's totems.
        /// </summary>
        IEntityBuilder Totem { get; }

        /// <summary>
        /// Gets an entity representing the character's minions.
        /// </summary>
        IEntityBuilder Minion { get; }

        IEntityBuilder From(IEnumerable<Entity> entities);
    }
}