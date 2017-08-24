namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IPoolStatBuilders
    {
        IPoolStatBuilder Life { get; }
        IPoolStatBuilder Mana { get; }
        IPoolStatBuilder EnergyShield { get; }
    }
}