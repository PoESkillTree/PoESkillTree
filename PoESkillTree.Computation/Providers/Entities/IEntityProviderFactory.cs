namespace PoESkillTree.Computation.Providers.Entities
{
    public interface IEntityProviderFactory
    {
        ISelfProvider Self { get; }
        IEnemyProvider Enemy { get; }
        IEntityProvider Ally { get; }

        ISkillEntityProvider Totem { get; }
        ISkillEntityProvider Minion { get; }

        IEntityProvider Any { get; }
    }
}