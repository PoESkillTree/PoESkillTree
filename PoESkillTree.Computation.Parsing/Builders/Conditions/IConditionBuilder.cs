namespace PoESkillTree.Computation.Parsing.Builders.Conditions
{
    public interface IConditionBuilder
    {
        IConditionBuilder And(IConditionBuilder condition);
        IConditionBuilder Or(IConditionBuilder condition);
        IConditionBuilder Not { get; }
    }
}