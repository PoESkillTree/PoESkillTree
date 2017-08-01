using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IReferencedMatchers<T>
    {
        IReadOnlyList<(string regex, T match)> Matchers { get; }
    }
}