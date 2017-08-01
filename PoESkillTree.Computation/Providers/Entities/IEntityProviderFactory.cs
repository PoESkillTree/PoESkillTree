namespace PoESkillTree.Computation.Providers.Entities
{
    public interface IEntityProviderFactory
    {
        // The entity that is modified by the stat, i.e. Character by default and others with
        // IConditionProviderFactory.For() or IEntityProvider.Stat()
        ISelfProvider Self { get; }
        IEnemyProvider Enemy { get; }
        IEntityProvider Ally { get; }
        // Explicitly refers to the player character. Only use this Self on a modifier does not
        // refer to Character but you still need to reference the player character.
        IEntityProvider Character { get; }

        ISkillEntityProvider Totem { get; }
        ISkillEntityProvider Minion { get; }

        IEntityProvider Any { get; }
    }
}