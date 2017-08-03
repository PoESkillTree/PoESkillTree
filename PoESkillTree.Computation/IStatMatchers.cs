using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IStatMatchers
    {
        IReadOnlyList<MatcherData> Matchers { get; }
    }
}