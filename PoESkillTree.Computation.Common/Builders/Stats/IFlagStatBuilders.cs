using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for flag stats.
    /// </summary>
    public interface IFlagStatBuilders
    {
        IStatBuilder ShieldModifiersApplyToMinionsInstead { get; }

        IStatBuilder IgnoreHexproof { get; }
        IStatBuilder CriticalStrikeChanceIsLucky { get; }
        IStatBuilder FarShot { get; }

        IStatBuilder IncreasesToSourceApplyToTarget(IStatBuilder source, IStatBuilder target);

        IConditionBuilder AlwaysMoving { get; }
        IConditionBuilder AlwaysStationary { get; }

        IConditionBuilder IsBrandAttachedToEnemy { get; }
        IConditionBuilder IsBannerPlanted { get; }
        IConditionBuilder InBloodStance { get; }
        IConditionBuilder InSandStance { get; }
    }
}