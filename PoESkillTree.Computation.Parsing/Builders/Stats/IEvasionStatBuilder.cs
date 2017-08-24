namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IEvasionStatBuilder : IStatBuilder
    {
        IStatBuilder Chance { get; }
        IStatBuilder ChanceAgainstProjectileAttacks { get; }
        IStatBuilder ChanceAgainstMeleeAttacks { get; }
    }
}