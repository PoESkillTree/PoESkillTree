namespace PoESkillTree.Computation.Providers
{
    public interface IDamageSourceProvider
    {

    }


    public interface IDamageSourceProviderFactory
    {
        IDamageSourceProvider Attack { get; }
        IDamageSourceProvider Spell { get; }
        IDamageSourceProvider Secondary { get; }
        IDamageSourceProvider DamageOverTime { get; }
    }


    public static class DamageSourceProviders
    {
        public static readonly IDamageSourceProviderFactory Source;
    }
}