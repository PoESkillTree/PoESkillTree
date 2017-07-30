using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Values
{
    public interface IValueProviderFactory
    {
        IThenBuilder If(IConditionProvider condition);

        ValueProvider Create(double value);
    }
    
    public interface IThenBuilder
    {
        IConditionalValueBuilder Then(ValueProvider value);
        IConditionalValueBuilder Then(double value);
    }

    public interface IConditionalValueBuilder
    {
        IThenBuilder ElseIf(IConditionProvider condition);

        ValueProvider Else(ValueProvider value);
        ValueProvider Else(double value);
    }
}