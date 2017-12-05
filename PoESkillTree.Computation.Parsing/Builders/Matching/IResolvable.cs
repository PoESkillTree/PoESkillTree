using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public interface IResolvable<out T>
    {
        T Resolve(ResolveContext context);
    }

    public delegate T Resolver<T>(T current, ResolveContext context);

    public class ResolveContext
    {
        public ResolveContext(
            IMatchContext<IValueBuilder> valueContext, IMatchContext<IReferenceConverter> referenceContext)
        {
            ValueContext = valueContext;
            ReferenceContext = referenceContext;
        }

        public IMatchContext<IValueBuilder> ValueContext { get; }

        public IMatchContext<IReferenceConverter> ReferenceContext { get; }

        private bool Equals(ResolveContext other)
        {
            return ValueContext.Equals(other.ValueContext) && ReferenceContext.Equals(other.ReferenceContext);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is ResolveContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ValueContext.GetHashCode() * 397) ^ ReferenceContext.GetHashCode();
            }
        }
    }
}