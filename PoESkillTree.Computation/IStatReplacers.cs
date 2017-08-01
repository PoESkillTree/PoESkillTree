using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IStatReplacers
    {
        IEnumerable<object> Replacers { get; }
    }
}