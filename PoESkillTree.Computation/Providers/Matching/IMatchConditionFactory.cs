using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers.Matching
{
    public interface IMatchConditionFactory
    {
        IMatchCondition MatchHas(ValueColoring valueColoring);
    }
}