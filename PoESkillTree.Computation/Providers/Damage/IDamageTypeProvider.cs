using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Entities;

namespace PoESkillTree.Computation.Providers.Damage
{
    public interface IDamageTypeProvider : IKeywordProvider
    {
        // Combinations (And, Invert, Except) and IKeywordProvider:
        // matches gem/skill if any of the keywords matches in the combination

        IDamageTypeProvider And(IDamageTypeProvider type);

        // e.g. fire -> (physical, lightning, cold, chaos)
        IDamageTypeProvider Invert { get; }

        // e.g. Elemental.Except(Fire) -> (Lightning, Cold)
        IDamageTypeProvider Except(IDamageTypeProvider type);

        IStatProvider Resistance { get; }

        IDamageStatProvider Damage { get; }

        IConditionProvider DamageOverTimeIsOn(IEntityProvider entity);

        IStatProvider Penetration { get; }
        // Damage done ignores enemy resistances of this type
        IFlagStatProvider IgnoreResistance { get; }
    }
}