using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Data.Base
{
    /// <summary>
    /// Base class for matcher implementations providing access to <see cref="IBuilderFactories"/> and 
    /// <see cref="IValueBuilders"/>.
    /// </summary>
    /// <remarks>
    /// Properties and methods being the same as those of <see cref="IBuilderFactories"/> (and nested classes)
    /// are not documented here again. See the original interface for their documentation.
    /// </remarks>
    public abstract class UsesValueBuilders
    {
        protected UsesValueBuilders(IBuilderFactories builderFactories)
        {
            BuilderFactories = builderFactories;
        }

        protected IBuilderFactories BuilderFactories { get; }

        protected IValueBuilders ValueFactory => BuilderFactories.ValueBuilders;
    }
}