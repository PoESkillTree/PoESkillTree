using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common.Builders.Resolving
{
    /// <summary>
    /// Interface for objects that can be resolved using a <see cref="ResolveContext"/>.
    /// </summary>
    /// <typeparam name="T">The resulting type of resolving. Generally this is the type that is implementing this
    /// interface.</typeparam>
    public interface IResolvable<out T>
    {
        /// <summary>
        /// Resolves this instance using the given match context.
        /// </summary>
        T Resolve(ResolveContext context);
    }

    /// <summary>
    /// Class holding the context instances required for <see cref="IResolvable{T}.Resolve"/>.
    /// </summary>
    public class ResolveContext : ValueObject
    {
        public ResolveContext(
            IMatchContext<IValueBuilder> valueContext, IMatchContext<IReferenceConverter> referenceContext)
        {
            ValueContext = valueContext;
            ReferenceContext = referenceContext;
        }

        /// <summary>
        /// Gets the context holding the resolved values.
        /// </summary>
        public IMatchContext<IValueBuilder> ValueContext { get; }

        /// <summary>
        /// Gets the context holding the resolved references.
        /// </summary>
        public IMatchContext<IReferenceConverter> ReferenceContext { get; }

        protected override object ToTuple() => (ValueContext, ReferenceContext);
    }
}