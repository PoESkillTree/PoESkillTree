using PoESkillTree.Computation.Common.Builders.Entities;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents a stat for damage. Damage can be limited by damage type and everything from
    /// <see cref="IDamageRelatedStatBuilder"/>.
    /// </summary>
    public interface IDamageStatBuilder : IDamageRelatedStatBuilder
    {
        new IDamageStatBuilder For(IEntityBuilder entity);

        /// <summary>
        /// Gets a stat representing the modifier to damage taken of this stat's damage types.
        /// </summary>
        IDamageRelatedStatBuilder Taken { get; }
    }
}