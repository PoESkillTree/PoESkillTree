namespace PoESkillTree.Computation.Providers
{
    public interface IDamageSourceProvider
    {

    }

    public static class DamageSourceProviders
    {
        public static readonly IDamageSourceProvider Attack;
        public static readonly IDamageSourceProvider Spell;
        public static readonly IDamageSourceProvider Secondary;
        public static readonly IDamageSourceProvider DamageOverTime;
    }
}