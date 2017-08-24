using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
{
    public interface IBuffBuilder : IEffectBuilder
    {
        IStatBuilder EffectIncrease { get; }

        // action to gain/apply the buff
        IActionBuilder<ISelfBuilder, IEntityBuilder> Action { get; }
    }
}