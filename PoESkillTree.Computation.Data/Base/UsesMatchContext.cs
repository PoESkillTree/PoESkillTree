using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesMatchContext : UsesConditionProviders
    {
        private readonly IMatchContextFactory _matchContextFactory;

        protected UsesMatchContext(IProviderFactories providerFactories,
            IMatchContextFactory matchContextFactory)
            : base(providerFactories)
        {
            _matchContextFactory = matchContextFactory;
        }

        protected IMatchContext<IGroupConverter> Groups => _matchContextFactory.Groups;
        protected IGroupConverter Group => Groups.Single;

        protected IMatchContext<ValueProvider> Values => _matchContextFactory.Values;
        protected ValueProvider Value => Values.Single;
    }
}