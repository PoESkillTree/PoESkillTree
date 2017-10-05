using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IStatMatchers
    {
        IEnumerable<MatcherData> Matchers { get; }

        bool MatchesWholeLineOnly { get; }
    }
}