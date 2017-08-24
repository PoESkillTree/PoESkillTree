using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Base
{
    public abstract class UsesValueProviders
    {
        protected UsesValueProviders(IBuilderFactories builderFactories)
        {
            BuilderFactories = builderFactories;
        }

        protected IBuilderFactories BuilderFactories { get; }

        protected IValueBuilders ValueFactory => BuilderFactories.ValueBuilders;
    }
}