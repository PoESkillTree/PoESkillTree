using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IStatFactory
    {
        IStat ChanceToDouble(IStat stat);

        IEnumerable<IStat> ConvertTo(IStat sourceStat, IEnumerable<IStat> targetStats);
        IEnumerable<IStat> GainAs(IStat sourceStat, IEnumerable<IStat> targetStats);
    }
}