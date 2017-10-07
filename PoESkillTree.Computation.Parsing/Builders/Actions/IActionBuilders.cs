using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    public interface IActionBuilders
    {
        ISelfToAnyActionBuilder Kill { get; }
        IBlockActionBuilder Block { get; }
        ISelfToAnyActionBuilder Hit { get; }
        ISelfToAnyActionBuilder SavageHit { get; }
        ICriticalStrikeActionBuilder CriticalStrike { get; }
        ISelfToAnyActionBuilder NonCriticalStrike { get; }
        ISelfToAnyActionBuilder Shatter { get; }
        ISelfToAnyActionBuilder ConsumeCorpse { get; }

        ISelfToAnyActionBuilder SpendMana(IValueBuilder amount);
    }
}