using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Damage
{
    public interface IDamageTypeBuilder : IKeywordBuilder
    {
        // Combinations (And, Invert, Except) and IKeywordBuilder:
        // matches gem/skill if any of the keywords matches in the combination

        IDamageTypeBuilder And(IDamageTypeBuilder type);

        // e.g. fire -> (physical, lightning, cold, chaos)
        IDamageTypeBuilder Invert { get; }

        // e.g. Elemental.Except(Fire) -> (Lightning, Cold)
        IDamageTypeBuilder Except(IDamageTypeBuilder type);

        IStatBuilder Resistance { get; }

        IDamageStatBuilder Damage { get; }

        IConditionBuilder DamageOverTimeIsOn(IEntityBuilder entity);

        IStatBuilder Penetration { get; }
        // Damage done ignores enemy resistances of this type
        IFlagStatBuilder IgnoreResistance { get; }
    }
}