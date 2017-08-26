using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    // these can only have value 0 or 1
    public interface IFlagStatBuilder : IStatBuilder
    {
        // shortcut for Value == 1
        IConditionBuilder IsSet { get; }

        IStatBuilder Effect { get; }

        // Applies to buffs that grant this flag
        IStatBuilder Duration { get; }
    }
}