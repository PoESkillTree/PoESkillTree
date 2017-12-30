using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for stat matcher implementations providing direct access to the properties of
    /// <see cref="IMatchContexts"/>.
    /// </summary>
    public abstract class UsesMatchContext : UsesConditionBuilders
    {
        private readonly IMatchContexts _matchContexts;

        protected UsesMatchContext(IBuilderFactories builderFactories,
            IMatchContexts matchContexts)
            : base(builderFactories)
        {
            _matchContexts = matchContexts;
        }

        protected IMatchContext<IReferenceConverter> References => _matchContexts.References;

        /// <summary>
        /// Shortcut for <c>References.Single</c>.
        /// </summary>
        protected IReferenceConverter Reference => References.Single;

        protected IMatchContext<ValueBuilder> Values => _matchContexts.Values;

        /// <summary>
        /// Shortcut for <c>Values.Single</c>.
        /// </summary>
        protected ValueBuilder Value => Values.Single;
    }
}