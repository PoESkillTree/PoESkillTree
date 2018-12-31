namespace PoESkillTree.Computation.Common.Builders.Stats
{
    public interface IRequirementStatBuilders
    {
        IStatBuilder Level { get; }
        IStatBuilder Strength { get; }
        IStatBuilder Dexterity { get; }
        IStatBuilder Intelligence { get; }
    }
}