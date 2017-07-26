using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IGemModifierProvider
    {
        
    }

    public static class GemModifierProviders
    {
        public static IGemModifierProvider IncreaseLevelBy(ValueProvider value,
            bool onlySocketedGems = false, bool onlySupportGems = false)
        {
            throw new NotImplementedException();
        }
    }
}