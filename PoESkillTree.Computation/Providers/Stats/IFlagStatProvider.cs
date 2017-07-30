using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Stats
{
    // these can only have value 0 or 1
    public interface IFlagStatProvider : IStatProvider
    {
        // shortcut for Value == 1
        IConditionProvider IsSet { get; }

        IStatProvider EffectIncrease { get; }

        // Applies to buffs that grant this flag
        IStatProvider DurationIncrease { get; }
    }
}