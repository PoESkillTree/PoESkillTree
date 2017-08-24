using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    public interface ICriticalStrikeActionBuilder : ISelfToAnyActionBuilder
    {
        IStatBuilder Chance { get; }

        IStatBuilder Multiplier { get; }

        IStatBuilder AilmentMultiplier { get; }

        IStatBuilder ExtraDamageTaken { get; }
    }
}