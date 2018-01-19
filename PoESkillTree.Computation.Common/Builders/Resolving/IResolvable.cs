using PoESkillTree.Computation.Common.Builders.Values;

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
    public class ResolveContext
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

        public override bool Equals(object obj) =>
            (this == obj) || (obj is ResolveContext other && Equals(other));

        private bool Equals(ResolveContext other) =>
            ValueContext.Equals(other.ValueContext) && ReferenceContext.Equals(other.ReferenceContext);

        public override int GetHashCode() =>
            (ValueContext, ReferenceContext).GetHashCode();
    }
}