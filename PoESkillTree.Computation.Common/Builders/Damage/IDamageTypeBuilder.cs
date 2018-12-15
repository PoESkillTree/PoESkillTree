using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Damage
{
    /// <summary>
    /// Represents one or a collection of damage types, e.g. Physical or Fire.
    /// </summary>
    /// <remarks>
    /// Collections of damage types are handled as keywords by checking whether the gem/skill in question has a keyword
    /// that is a damage type in the collection.
    /// <para>Most stats of damage types only make sense for single damage types, not for collections. However, as
    /// modifiers to stats they do make sense on collections.</para>
    /// </remarks>
    public interface IDamageTypeBuilder : IKeywordBuilder
    {
        /// <summary>
        /// Returns a collection of damage types that contains the union of this instance's and 
        /// <paramref name="type"/>'s damage types.
        /// <para>E.g. (Fire, Cold).And(Fire, Chaos) -> (Fire, Cold, Chaos)</para>
        /// </summary>
        IDamageTypeBuilder And(IDamageTypeBuilder type);

        /// <summary>
        /// Gets a collection that contains all damage types not in this collection (except RandomElement).
        /// <para>E.g. (Fire, Cold).Invert -> (Physical, Lighting, Chaos)</para>
        /// </summary>
        IDamageTypeBuilder Invert { get; }

        /// <summary>
        /// Returns a collection that contains all damage types of this collection that are not contained in
        /// <paramref name="type"/>.
        /// <para>E.g. (Fire, Lightning, Cold).Except(Fire) -> (Lightning, Cold)</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IDamageTypeBuilder Except(IDamageTypeBuilder type);

        /// <summary>
        /// Gets a stat representing the resistances to the damage types in this collection.
        /// </summary>
        IStatBuilder Resistance { get; }

        /// <summary>
        /// Gets a damage stat representing the damage of the damage types in this collection.
        /// </summary>
        IDamageStatBuilder Damage { get; }

        /// <summary>
        /// Gets a stat representing the multiplier to damage of this type.
        /// </summary>
        IDamageRelatedStatBuilder DamageMultiplier { get; }

        /// <summary>
        /// Starts constructing a stat representing the percentage of damage of this stat's damage types that is taken
        /// from the given pool before being taken from another pool.
        /// </summary>
        IDamageTakenConversionBuilder DamageTakenFrom(IPoolStatBuilder pool);

        /// <summary>
        /// Returns a stat representing the percentage of hit damage of the damage types in this collection that is
        /// taken as the given damage type instead.
        /// </summary>
        IStatBuilder HitDamageTakenAs(DamageType type);

        /// <summary>
        /// Gets a stat representing the amount of enemy resistances of the damage types in this collection penetrated
        /// by damage.
        /// </summary>
        IDamageRelatedStatBuilder Penetration { get; }
        IDamageRelatedStatBuilder PenetrationWithCrits { get; }
        IDamageRelatedStatBuilder PenetrationWithNonCrits { get; }

        /// <summary>
        /// Gets a stat representing whether damage ignores enemy resistances of the damage types in this collection.
        /// </summary>
        IDamageRelatedStatBuilder IgnoreResistance { get; }
        IDamageRelatedStatBuilder IgnoreResistanceWithCrits { get; }
        IDamageRelatedStatBuilder IgnoreResistanceWithNonCrits { get; }

        IStatBuilder ReflectedDamageTaken { get; }

        IReadOnlyList<DamageType> BuildDamageTypes(BuildParameters parameters);
    }

    public interface IDamageTakenConversionBuilder
    {
        /// <summary>
        /// Returns a stat representing the percentage of damage of specific types that is taken from a specific pool
        /// before being taken from the given pool.
        /// </summary>
        IStatBuilder Before(IPoolStatBuilder pool);
    }
}