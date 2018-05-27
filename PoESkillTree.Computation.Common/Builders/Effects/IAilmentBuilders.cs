namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <summary>
    /// Factory interface for ailments.
    /// </summary>
    public interface IAilmentBuilders
    {
        IAilmentBuilder Ignite { get; }
        IAilmentBuilder Shock { get; }
        IAilmentBuilder Chill { get; }
        IAilmentBuilder Freeze { get; }

        IAilmentBuilder Bleed { get; }
        IAilmentBuilder Poison { get; }

        /// <summary>
        /// Gets a collection of all elemental ailments.
        /// </summary>
        IAilmentBuilderCollection Elemental { get; }
    }
}