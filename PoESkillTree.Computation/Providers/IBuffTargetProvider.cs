namespace PoESkillTree.Computation.Providers
{
    public interface IBuffTargetProvider
    {

    }

    public static class BuffTargetProviders
    {
        public static readonly IBuffTargetProvider Self;
        public static readonly IBuffTargetProvider Enemy;
    }
}