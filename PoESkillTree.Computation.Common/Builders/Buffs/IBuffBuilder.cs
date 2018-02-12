using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Buffs
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