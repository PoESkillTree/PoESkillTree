using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Conditions
{
    public interface IConditionBuilder : IResolvable<IConditionBuilder>
    {
        IConditionBuilder And(IConditionBuilder condition);
        IConditionBuilder Or(IConditionBuilder condition);
        IConditionBuilder Not { get; }
    }
}