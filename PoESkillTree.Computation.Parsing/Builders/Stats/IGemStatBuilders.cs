namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IGemStatBuilders
    {
        IStatBuilder IncreaseLevel(bool onlySupportGems = false);
    }
}