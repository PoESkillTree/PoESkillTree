using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Actions
{
    public interface ICriticalStrikeActionProvider : ISelfToAnyActionProvider
    {
        IStatProvider Chance { get; }

        IStatProvider Multiplier { get; }

        IStatProvider AilmentMultiplier { get; }

        // default value: 30% (default monster crit multi is 130%)
        // TODO must be specified somewhere (along with other monster stats)
        IStatProvider ExtraDamageTaken { get; }
    }
}