using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IAvoidableEffectBuilder : IEffectBuilder
    {
        IStatBuilder Avoidance { get; }
    }
}