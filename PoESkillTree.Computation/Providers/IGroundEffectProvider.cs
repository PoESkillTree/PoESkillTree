namespace PoESkillTree.Computation.Providers
{
    public interface IGroundEffectProvider : IEffectProvider
    {
        
    }


    public interface IGroundEffectProviderFactory
    {
        IGroundEffectProvider Consecrated { get; }
    }


    public static class GroundEffectProviders
    {
        public static readonly IGroundEffectProviderFactory Ground;
    }
}