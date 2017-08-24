using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IKnockbackEffectBuilder : IEffectBuilder
    {
        IStatBuilder Distance { get; }
    }
}