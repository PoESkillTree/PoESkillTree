using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IReferencedMatchers<T>
    {
        string ReferenceName { get; }

        IReadOnlyList<ReferencedMatcherData<T>> Matchers { get; }
    }
}