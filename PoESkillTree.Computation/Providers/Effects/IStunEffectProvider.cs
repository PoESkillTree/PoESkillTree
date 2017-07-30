using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IStunEffectProvider : IAvoidableEffectProvider, 
        IActionProvider<ISelfProvider, IEnemyProvider>
    {
        IStatProvider Threshold { get; }

        IStatProvider Recovery { get; }

        IStatProvider ChanceToAvoidInterruptionWhileCasting { get; }
    }
}