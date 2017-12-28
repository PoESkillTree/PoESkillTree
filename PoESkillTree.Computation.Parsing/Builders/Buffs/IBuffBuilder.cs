using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
{
    /// <summary>
    /// Represents buff effects.
    /// </summary>
    public interface IBuffBuilder : IEffectBuilder
    {
        /// <summary>
        /// Gets a stat representing the effect modifier of this buff.
        /// </summary>
        IStatBuilder Effect { get; }

        /// <summary>
        /// Gets an action that occurs when Self gains this buff.
        /// </summary>
        IActionBuilder Action { get; }
    }
}