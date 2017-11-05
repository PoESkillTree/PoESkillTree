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
    }
}