using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
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

        // default maximum value is 1 for everything except poison
        // default maximum value is positive infinity for poison
        /// <summary>
        /// Returns a stat representing the number of instances of this ailment currently affecting
        /// <paramref name="target"/>.
        /// </summary>
        IStatBuilder InstancesOn(IEntityBuilder target);

        /// <summary>
        /// Returns a flag stat representing whether <paramref name="type"/> can inflict this ailment.
        /// </summary>
        IFlagStatBuilder Source(IDamageTypeBuilder type);

        /// <summary>
        /// Returns a flag stat representing whether the types in <paramref name="types"/> can inflict this ailment.
        /// </summary>
        IFlagStatBuilder Sources(IEnumerable<IDamageTypeBuilder> types);
    }
}