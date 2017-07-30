using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Buffs
{
    public interface IBuffProviderCollection : IProviderCollection<IBuffProvider>
    {
        IStatProvider CombinedLimit { get; }
        IStatProvider EffectIncrease { get; }

        IBuffProviderCollection ExceptFrom(params ISkillProvider[] skills);

        IBuffProviderCollection With(IKeywordProvider keyword);
        IBuffProviderCollection Without(IKeywordProvider keyword);
    }
}