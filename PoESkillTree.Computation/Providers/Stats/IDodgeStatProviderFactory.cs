namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IDodgeStatProviderFactory
    {
        IStatProvider AttackChance { get; }
        IStatProvider SpellChance { get; }
    }
}