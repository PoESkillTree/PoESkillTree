using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    public interface IEntityBuilder
    {
        IConditionBuilder HitByInPastXSeconds(IDamageTypeBuilder damageType, 
            ValueBuilder seconds);

        IConditionBuilder HitByInPastXSeconds(IDamageTypeBuilder damageType, double seconds);

        IConditionBuilder HitByRecently(IDamageTypeBuilder damageType);

        // Changes the context of a stat in the same way as IConditionBuilders.For(target)
        T Stat<T>(T stat) where T : IStatBuilder;

        IStatBuilder Level { get; }
    }
}