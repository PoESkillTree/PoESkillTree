using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IStatMatchers
    {
        IEnumerable<object> StatMatchers { get; }
    }
}