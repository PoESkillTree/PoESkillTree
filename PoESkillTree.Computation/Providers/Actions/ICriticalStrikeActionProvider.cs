using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Actions
{
    public interface ICriticalStrikeActionProvider : ISelfToAnyActionProvider
    {
        IStatProvider Chance { get; }

        IStatProvider Multiplier { get; }

        IStatProvider AilmentMultiplier { get; }

        IStatProvider ExtraDamageTaken { get; }
    }
}