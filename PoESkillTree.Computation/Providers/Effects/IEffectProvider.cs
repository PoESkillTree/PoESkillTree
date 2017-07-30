using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IEffectProvider
    {
        IFlagStatProvider On(IEntityProvider target);

        // needs to be entered by user if this sets On(target) to 1?
        // (default action is Hit if non is specified)
        IStatProvider ChanceOn(IEntityProvider target);

        // shortcut for On(target).IsSet
        IConditionProvider IsOn(IEntityProvider entity);

        // duration when source is Self
        IStatProvider Duration { get; }
    }
}