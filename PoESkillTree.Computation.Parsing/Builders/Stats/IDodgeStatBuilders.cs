namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IDodgeStatBuilders
    {
        IStatBuilder AttackChance { get; }
        IStatBuilder SpellChance { get; }
    }
}