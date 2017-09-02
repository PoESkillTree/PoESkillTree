using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public interface IValueBuilders
    {
        IThenBuilder If(IConditionBuilder condition);

        IValueBuilder Create(double value);

        Func<IValueBuilder, IValueBuilder> WrapValueConverter(
            Func<ValueBuilder, ValueBuilder> converter);
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