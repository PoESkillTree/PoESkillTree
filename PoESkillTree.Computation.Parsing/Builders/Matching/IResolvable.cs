using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public interface IResolvable<out T>
    {
        T Resolve(IMatchContext<IValueBuilder> valueContext);
    }

    public delegate T Resolver<T>(T current, IMatchContext<IValueBuilder> valueContext);
}