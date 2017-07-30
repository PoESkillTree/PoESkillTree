using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Buffs
{
    public interface IBuffProvider : IEffectProvider
    {
        IStatProvider EffectIncrease { get; }

        // action to gain/apply the buff
        IActionProvider<ISelfProvider, IEntityProvider> Action { get; }
    }
}