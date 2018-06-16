using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface IStatFactory
    {
        IStat FromIdentity(string identity, Entity entity, Type dataType, bool isExplicitlyRegistered = false);

        IStat ChanceToDouble(IStat stat);

        IEnumerable<IStat> ConvertTo(IStat sourceStat, IEnumerable<IStat> targetStats);
        IEnumerable<IStat> GainAs(IStat sourceStat, IEnumerable<IStat> targetStats);
        IStat ConvertTo(IStat source, IStat target);
        IStat GainAs(IStat source, IStat target);
        IStat Conversion(IStat source);
        IStat SkillConversion(IStat source);
    }
}