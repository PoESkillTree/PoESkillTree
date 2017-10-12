using System;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IReferencedMatchers : IEnumerable<ReferencedMatcherData>
    {
        string ReferenceName { get; }

        Type MatchType { get; }
    }
}