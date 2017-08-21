using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesValueProviders
    {
        protected UsesValueProviders(IProviderFactories providerFactories)
        {
            ProviderFactories = providerFactories;
        }

        protected IProviderFactories ProviderFactories { get; }

        protected IValueProviderFactory ValueFactory => ProviderFactories.ValueProviderFactory;
    }
}