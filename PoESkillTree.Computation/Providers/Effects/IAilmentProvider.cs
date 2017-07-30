using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IAilmentProvider : IAvoidableEffectProvider
    {
        // shortcut for ChanceOn(Enemy)
        IStatProvider Chance { get; }

        // default value is 1 for everything except bleed
        // default value is positive infinity for bleed
        IStatProvider InstancesOn(IEntityProvider target);

        IFlagStatProvider AddSource(IDamageTypeProvider type);
        IFlagStatProvider AddSources(IEnumerable<IDamageTypeProvider> types);
    }
}