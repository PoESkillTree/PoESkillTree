namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IAttributeStatBuilders
    {
        IStatBuilder Strength { get; }
        IStatBuilder Dexterity { get; }
        IStatBuilder Intelligence { get; }

        IStatBuilder StrengthDamageBonus { get; }
        IStatBuilder DexterityEvasionBonus { get; }
    }
}