using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesValueProviders
    {
        protected UsesValueProviders(IProviderFactories providerFactories)
        {
            ValueFactory = providerFactories.ValueProviderFactory;
        }

        protected IValueProviderFactory ValueFactory { get; }
    }
}