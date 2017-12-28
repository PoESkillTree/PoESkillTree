using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    /// <summary>
    /// Factory interface for actions.
    /// </summary>
    public interface IActionBuilders
    {
        /// <summary>
        /// Gets an action that occurs when Self kills any entity.
        /// </summary>
        IActionBuilder Kill { get; }

        /// <summary>
        /// Gets an action that occurs when Self blocks a hit from any entity.
        /// </summary>
        IBlockActionBuilder Block { get; }

        /// <summary>
        /// Gets an action that occurs when Self hits any entity.
        /// </summary>
        IActionBuilder Hit { get; }

        /// <summary>
        /// Gets an action that occurs when Self savagely hits any entity.
        /// </summary>
        IActionBuilder SavageHit { get; }

        /// <summary>
        /// Gets an action that occurs when Self critically hits any entity.
        /// </summary>
        ICriticalStrikeActionBuilder CriticalStrike { get; }

        /// <summary>
        /// Gets an action that occurs when Self non-critically hits any entity.
        /// </summary>
        IActionBuilder NonCriticalStrike { get; }

        /// <summary>
        /// Gets an action that occurs when Self shatters any entity.
        /// </summary>
        IActionBuilder Shatter { get; }

        /// <summary>
        /// Gets an action that occurs when Self consumes the corpse of any entity.
        /// </summary>
        IActionBuilder ConsumeCorpse { get; }

        /// <summary>
        /// Gets an action that occurs when Self spends <paramref name="amount"/> mana.
        /// </summary>
        IActionBuilder SpendMana(IValueBuilder amount);
    }
}