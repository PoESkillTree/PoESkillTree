using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Matching
{
    public interface IMatchContextFactory
    {
        // includes only regex groups of from ({FooMatchers})
        IMatchContext<IGroupConverter> Groups { get; }

        IMatchContext<ValueProvider> Values { get; }
    }
}