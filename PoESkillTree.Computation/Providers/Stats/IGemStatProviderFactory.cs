namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IGemStatProviderFactory
    {
        IStatProvider IncreaseLevel(bool onlySupportGems = false);
    }
}