using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Values
{
    public interface IFluentValueBuilder
    {
        IThenBuilder If(IConditionProvider condition);
    }
    
    public interface IThenBuilder
    {
        IConditionalValueBuilder Then(ValueProvider value);
    }

    public interface IConditionalValueBuilder
    {
        IThenBuilder ElseIf(IConditionProvider condition);

        ValueProvider Else(ValueProvider value);
    }
}