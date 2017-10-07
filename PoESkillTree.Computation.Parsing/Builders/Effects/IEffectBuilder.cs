using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IEffectBuilder : IResolvable<IEffectBuilder>
    {
        IFlagStatBuilder On(IEntityBuilder target);

        // needs to be entered by user if this sets On(target) to 1?
        // (default action is Hit if non is specified)
        IStatBuilder ChanceOn(IEntityBuilder target);

        // shortcut for On(target).IsSet
        IConditionBuilder IsOn(IEntityBuilder target);

        // duration when source is Self
        IStatBuilder Duration { get; }
    }
}