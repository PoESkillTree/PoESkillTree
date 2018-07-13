using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Buffs
{
    /// <summary>
    /// Represents buff effects.
    /// </summary>
    public interface IBuffBuilder : IEffectBuilder
    {
        /// <summary>
        /// Returns a flag stat representing whether <paramref name="target"/> is currently affected by this effect.
        /// This affection does not count as a buff and generic buff effect modifiers do not apply.
        /// </summary>
        IFlagStatBuilder NotAsBuffOn(IEntityBuilder target);

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