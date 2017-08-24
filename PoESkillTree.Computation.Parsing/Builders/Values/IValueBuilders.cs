using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public interface IValueBuilders
    {
        IThenBuilder If(IConditionBuilder condition);

        ValueBuilder Create(double value);
    }
    
    public interface IThenBuilder
    {
        IConditionalValueBuilder Then(ValueBuilder value);
        IConditionalValueBuilder Then(double value);
    }

    public interface IConditionalValueBuilder
    {
        IThenBuilder ElseIf(IConditionBuilder condition);

        ValueBuilder Else(ValueBuilder value);
        ValueBuilder Else(double value);
    }
}