namespace PoESkillTree.Computation.Parsing.Builders.Damage
{
    public interface IDamageTypeBuilders
    {
        IDamageTypeBuilder Physical { get; }

        IDamageTypeBuilder Fire { get; }

        IDamageTypeBuilder Lightning { get; }

        IDamageTypeBuilder Cold { get; }

        IDamageTypeBuilder Chaos { get; }

        IDamageTypeBuilder RandomElement { get; }
    }
}