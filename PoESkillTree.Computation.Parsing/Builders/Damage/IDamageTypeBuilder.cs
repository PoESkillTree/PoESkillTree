using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Damage
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
        /// Gets a collection that contains all damage types not in this collection.
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
        /// Returns a collection that is satisfied if <paramref name="entity"/> is affected by damage over time
        /// of any damage type in this collection.
        /// </summary>
        IConditionBuilder DamageOverTimeIsOn(IEntityBuilder entity);

        /// <summary>
        /// Gets a stat representing the amount of enemy resistances of the damage types in this collection penetrated
        /// by damage.
        /// </summary>
        IStatBuilder Penetration { get; }

        /// <summary>
        /// Gets a flag stat representing whether damage ignores enemy resistances of the damage types in this
        /// collection.
        /// </summary>
        IFlagStatBuilder IgnoreResistance { get; }
    }
}