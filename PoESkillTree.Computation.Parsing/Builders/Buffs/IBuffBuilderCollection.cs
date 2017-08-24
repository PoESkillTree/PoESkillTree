using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
{
    public interface IBuffBuilderCollection : IBuilderCollection<IBuffBuilder>
    {
        IStatBuilder CombinedLimit { get; }
        IStatBuilder EffectIncrease { get; }

        IBuffBuilderCollection ExceptFrom(params ISkillBuilder[] skills);

        IBuffBuilderCollection With(IKeywordBuilder keyword);
        IBuffBuilderCollection Without(IKeywordBuilder keyword);
    }
}