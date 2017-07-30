namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IAilmentProviderFactory
    {
        IAilmentProvider Ignite { get; }
        IAilmentProvider Shock { get; }
        IAilmentProvider Chill { get; }
        IAilmentProvider Freeze { get; }

        IAilmentProvider Bleed { get; }
        IAilmentProvider Poison { get; }

        IAilmentProviderCollection All { get; }
        IAilmentProviderCollection Elemental { get; }
    }
}