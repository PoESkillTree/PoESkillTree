namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    public interface IEntityBuilders
    {
        // The entity that is modified by the stat, i.e. Character by default and others with
        // IConditionBuilders.For() or IEntityBuilder.Stat()
        ISelfBuilder Self { get; }
        IEnemyBuilder Enemy { get; }
        IEntityBuilder Ally { get; }
        // Explicitly refers to the player character. Only use this if Self on a modifier does not
        // refer to Character but you still need to reference the player character.
        IEntityBuilder Character { get; }

        ISkillEntityBuilder Totem { get; }
        ISkillEntityBuilder Minion { get; }

        IEntityBuilder Any { get; }
    }
}