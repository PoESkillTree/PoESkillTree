using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IEffectStats
    {
        IEnumerable<object> Effects { get; }

        IEnumerable<object> Flags { get; }
    }
}