using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Entities
{
    public interface IEnemyProvider : IEntityProvider
    {
        IConditionProvider IsNearby { get; }

        IConditionProvider IsRare { get; }
        IConditionProvider IsUnique { get; }
        IConditionProvider IsRareOrUnique { get; }
    }
}