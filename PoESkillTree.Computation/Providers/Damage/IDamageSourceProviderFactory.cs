namespace PoESkillTree.Computation.Providers.Damage
{
    public interface IDamageSourceProviderFactory
    {
        IDamageSourceProvider Attack { get; }
        IDamageSourceProvider Spell { get; }
        IDamageSourceProvider Secondary { get; }
        IDamageSourceProvider DamageOverTime { get; }
    }
}