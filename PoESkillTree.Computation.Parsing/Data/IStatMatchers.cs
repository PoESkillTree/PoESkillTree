using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IStatMatchers
    {
        IReadOnlyList<MatcherData> Matchers { get; }
    }
}