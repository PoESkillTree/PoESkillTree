using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IAilmentBuilder : IAvoidableEffectBuilder
    {
        // shortcut for ChanceOn(Enemy)
        IStatBuilder Chance { get; }

        // default maximum value is 1 for everything except poison
        // default maximum value is positive infinity for poison
        IStatBuilder InstancesOn(IEntityBuilder target);

        IFlagStatBuilder AddSource(IDamageTypeBuilder type);
        IFlagStatBuilder AddSources(IEnumerable<IDamageTypeBuilder> types);
    }
}