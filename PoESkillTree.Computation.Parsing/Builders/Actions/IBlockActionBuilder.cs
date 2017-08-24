using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    public interface IBlockActionBuilder : ISelfToAnyActionBuilder
    {
        IStatBuilder Recovery { get; }

        IStatBuilder AttackChance { get; }
        IStatBuilder SpellChance { get; }
    }
}