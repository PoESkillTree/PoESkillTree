using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Damage;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Entities
{
    public interface IEntityProvider
    {
        IConditionProvider HitByInPastXSeconds(IDamageTypeProvider damageType, 
            ValueProvider seconds);

        IConditionProvider HitByInPastXSeconds(IDamageTypeProvider damageType, double seconds);

        IConditionProvider HitByRecently(IDamageTypeProvider damageType);

        // Changes the context of a stat in the same way as IConditionProviderFactory.For(target)
        T Stat<T>(T stat) where T : IStatProvider;

        IStatProvider Level { get; }
    }
}