namespace PoESkillTree.Computation.Providers
{
    public interface IDamageTypeProvider
    {
        IDamageTypeProvider And(IDamageTypeProvider type);
    }

    public static class DamageTypeProviders
    {
        public static readonly IDamageTypeProvider Physical;
        public static readonly IDamageTypeProvider Fire;
        public static readonly IDamageTypeProvider Lightning;
        public static readonly IDamageTypeProvider Cold;
        public static readonly IDamageTypeProvider Elemental = Fire.And(Lightning).And(Cold);
        public static readonly IDamageTypeProvider Chaos;
    }
}