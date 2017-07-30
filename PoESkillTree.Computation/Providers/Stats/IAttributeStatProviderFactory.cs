namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IAttributeStatProviderFactory
    {
        IStatProvider Strength { get; }
        IStatProvider Dexterity { get; }
        IStatProvider Intelligence { get; }

        IStatProvider StrengthDamageBonus { get; }
        IStatProvider DexterityEvasionBonus { get; }
    }
}