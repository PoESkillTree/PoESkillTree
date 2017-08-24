namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    public interface IAilmentBuilders
    {
        IAilmentBuilder Ignite { get; }
        IAilmentBuilder Shock { get; }
        IAilmentBuilder Chill { get; }
        IAilmentBuilder Freeze { get; }

        IAilmentBuilder Bleed { get; }
        IAilmentBuilder Poison { get; }

        IAilmentBuilderCollection All { get; }
        IAilmentBuilderCollection Elemental { get; }
    }
}