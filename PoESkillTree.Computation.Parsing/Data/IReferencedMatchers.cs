using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IReferencedMatchers<T>
    {
        string ReferenceName { get; }

        IReadOnlyList<ReferencedMatcherData<T>> Matchers { get; }
    }
}