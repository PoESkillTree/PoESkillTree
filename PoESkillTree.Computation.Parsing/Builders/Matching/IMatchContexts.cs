using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public interface IMatchContexts
    {
        // includes only regex groups of from ({FooMatchers})
        IMatchContext<IGroupConverter> Groups { get; }

        IMatchContext<ValueBuilder> Values { get; }
    }
}