using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IStatReplacers
    {
        IReadOnlyList<StatReplacerData> Replacers { get; }
    }
}