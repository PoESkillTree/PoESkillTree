using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an ailment.
    /// </summary>
    public interface IAilmentBuilder : IAvoidableEffectBuilder
    {
        /// <summary>
        /// Gets a stat representing the chance to inflict this effect upon Enemies. This chance only applies to hits if
        /// not set differently via conditions.
        /// </summary>
        IStatBuilder Chance { get; }

        /// <summary>
        /// Returns a stat representing the number of instances of this ailment currently affecting
        /// <paramref name="target"/>.
        /// </summary>
        IStatBuilder InstancesOn(IEntityBuilder target);

        /// <summary>
        /// Returns a flag stat representing whether all of the damage types in <paramref name="type"/> can inflict
        /// this ailment.
        /// </summary>
        IFlagStatBuilder Source(IDamageTypeBuilder type);

        Ailment Build();
    }
}