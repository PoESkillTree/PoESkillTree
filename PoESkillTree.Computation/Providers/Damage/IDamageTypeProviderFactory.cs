namespace PoESkillTree.Computation.Providers.Damage
{
    public interface IDamageTypeProviderFactory
    {
        IDamageTypeProvider Physical { get; }

        IDamageTypeProvider Fire { get; }

        IDamageTypeProvider Lightning { get; }

        IDamageTypeProvider Cold { get; }

        IDamageTypeProvider Chaos { get; }

        IDamageTypeProvider RandomElement { get; }
    }
}