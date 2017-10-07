using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public interface IValueBuilders
    {
        IThenBuilder If(IConditionBuilder condition);

        IValueBuilder Create(double value);

        Func<IValueBuilder, IValueBuilder> WrapValueConverter(
            Func<ValueBuilder, ValueBuilder> converter);
    }
    
    public interface IThenBuilder : IResolvable<IThenBuilder>
    {
        IConditionalValueBuilder Then(IValueBuilder value);
        IConditionalValueBuilder Then(double value);
    }

    public interface IConditionalValueBuilder : IResolvable<IConditionalValueBuilder>
    {
        IThenBuilder ElseIf(IConditionBuilder condition);

        ValueBuilder Else(IValueBuilder value);
        ValueBuilder Else(double value);
    }
}