using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    public interface IEntityBuilder : IResolvable<IEntityBuilder>
    {
        IConditionBuilder HitByInPastXSeconds(IDamageTypeBuilder damageType, 
            IValueBuilder seconds);

        IConditionBuilder HitByInPastXSeconds(IDamageTypeBuilder damageType, double seconds);

        IConditionBuilder HitByRecently(IDamageTypeBuilder damageType);

        // Changes the context of a stat in the same way as IConditionBuilders.For(target)
        IDamageStatBuilder Stat(IDamageStatBuilder stat);
        IFlagStatBuilder Stat(IFlagStatBuilder stat);
        IPoolStatBuilder Stat(IPoolStatBuilder stat);
        IStatBuilder Stat(IStatBuilder stat);

        IStatBuilder Level { get; }
    }
}