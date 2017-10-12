using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IStatMatchers : IEnumerable<MatcherData>
    {
        IReadOnlyList<string> ReferenceNames { get; }

        bool MatchesWholeLineOnly { get; }
    }
}