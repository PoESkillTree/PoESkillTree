namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IEvasionStatProvider : IStatProvider
    {
        IStatProvider Chance { get; }
        IStatProvider ChanceAgainstProjectileAttacks { get; }
        IStatProvider ChanceAgainstMeleeAttacks { get; }
    }
}