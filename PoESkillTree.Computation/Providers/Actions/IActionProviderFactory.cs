using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Actions
{
    public interface IActionProviderFactory
    {
        ISelfToAnyActionProvider Kill { get; }
        IBlockActionProvider Block { get; }
        ISelfToAnyActionProvider Hit { get; }
        ISelfToAnyActionProvider SavageHit { get; }
        ICriticalStrikeActionProvider CriticalStrike { get; }
        ISelfToAnyActionProvider NonCriticalStrike { get; }
        ISelfToAnyActionProvider Shatter { get; }
        ISelfToAnyActionProvider ConsumeCorpse { get; }

        ISelfToAnyActionProvider SpendMana(ValueProvider amount);
    }
}