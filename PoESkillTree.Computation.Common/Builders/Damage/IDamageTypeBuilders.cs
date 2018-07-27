namespace PoESkillTree.Computation.Common.Builders.Damage
{
    /// <summary>
    /// Factory interface for damage types.
    /// </summary>
    public interface IDamageTypeBuilders
    {
        IDamageTypeBuilder Physical { get; }

        IDamageTypeBuilder Fire { get; }

        IDamageTypeBuilder Lightning { get; }

        IDamageTypeBuilder Cold { get; }

        IDamageTypeBuilder Chaos { get; }

        /// <summary>
        /// Gets a special elemental damage type that is handled differently depending on the user's settings.
        /// </summary>
        IDamageTypeBuilder RandomElement { get; }

        IDamageTypeBuilder From(DamageType damageType);
    }
}