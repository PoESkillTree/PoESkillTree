using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    public interface IEnemyBuilder : IEntityBuilder
    {
        IConditionBuilder IsNearby { get; }

        IConditionBuilder IsRare { get; }
        IConditionBuilder IsUnique { get; }
        IConditionBuilder IsRareOrUnique { get; }
    }
}