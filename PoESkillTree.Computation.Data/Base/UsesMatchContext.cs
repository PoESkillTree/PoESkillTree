using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesMatchContext : UsesConditionProviders
    {
        private readonly IMatchContexts _matchContexts;

        protected UsesMatchContext(IBuilderFactories builderFactories,
            IMatchContexts matchContexts)
            : base(builderFactories)
        {
            _matchContexts = matchContexts;
        }

        protected IMatchContext<IReferenceConverter> References => _matchContexts.References;
        protected IReferenceConverter Reference => References.Single;

        protected IMatchContext<ValueBuilder> Values => _matchContexts.Values;
        protected ValueBuilder Value => Values.Single;
    }
}