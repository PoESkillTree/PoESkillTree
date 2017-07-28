using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IGemModifierProvider
    {
        
    }

    public static class GemModifierProviders
    {
        // TODO will probably need some changes once other gem modifiers are added
        public static IGemModifierProvider IncreaseLevelBy(ValueProvider value,
            bool onlySocketedGems = false, bool onlySupportGems = false)
        {
            throw new NotImplementedException();
        }
    }
}