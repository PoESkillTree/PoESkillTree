using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Actions
{
    public interface IBlockActionProvider : ISelfToAnyActionProvider
    {
        IStatProvider Recovery { get; }

        IStatProvider AttackChance { get; }
        IStatProvider SpellChance { get; }
    }
}