using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IStunEffectBuilder : IAvoidableEffectBuilder, 
        IActionBuilder<ISelfBuilder, IEnemyBuilder>
    {
        IStatBuilder Threshold { get; }

        IStatBuilder Recovery { get; }

        IStatBuilder ChanceToAvoidInterruptionWhileCasting { get; }
    }
}