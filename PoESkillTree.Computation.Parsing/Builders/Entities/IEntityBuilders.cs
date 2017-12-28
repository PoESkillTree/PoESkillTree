namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    /// <summary>
    /// Factory interface for entities.
    /// </summary>
    public interface IEntityBuilders
    {
        /// <summary>
        /// Gets the entity whose stats are currently calculated. This normally is <see cref="ModififerSource"/>,
        /// except when stats apply to other entities via <see cref="Conditions.IConditionBuilders.For"/>.
        /// </summary>
        IEntityBuilder Self { get; }

        /// <summary>
        /// Gets an entity representing enemies.
        /// </summary>
        IEnemyBuilder Enemy { get; }

        /// <summary>
        /// Gets an entity representing allies.
        /// </summary>
        IEntityBuilder Ally { get; }

        /// <summary>
        /// Gets an entity explicitly representing the entity the modifier originates from.
        /// <para>Only use this if <see cref="Self"/> refers to another entity but you still need to reference the
        /// modifier's sourc entity.</para>
        /// <para>E.g. when applying stats from one entity to another or when a stat applies to both another entity and
        /// the modififer's source entity.</para>
        /// </summary>
        IEntityBuilder ModififerSource { get; }

        /// <summary>
        /// Gets an entity representing all of Self's totems.
        /// </summary>
        ISkillEntityBuilder Totem { get; }

        /// <summary>
        /// Gets an entity representing all of Self's minions.
        /// </summary>
        ISkillEntityBuilder Minion { get; }

        /// <summary>
        /// Gets an entity representing every entity.
        /// </summary>
        IEntityBuilder Any { get; }
    }
}