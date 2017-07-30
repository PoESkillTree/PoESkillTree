namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IPoolStatProviderFactory
    {
        IPoolStatProvider Life { get; }
        IPoolStatProvider Mana { get; }
        IPoolStatProvider EnergyShield { get; }
    }
}