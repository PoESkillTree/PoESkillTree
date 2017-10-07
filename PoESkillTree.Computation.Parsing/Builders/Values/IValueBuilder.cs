using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public interface IValueBuilder : IResolvable<IValueBuilder>
    {
        IConditionBuilder Eq(IValueBuilder other);
        IConditionBuilder Eq(double other);

        IConditionBuilder GreaterThen(IValueBuilder other);
        IConditionBuilder GreaterThen(double other);

        IValueBuilder Add(IValueBuilder other);
        IValueBuilder Add(double other);

        IValueBuilder Multiply(IValueBuilder other);
        IValueBuilder Multiply(double other);

        IValueBuilder AsDividend(IValueBuilder divisor);
        IValueBuilder AsDividend(double divisor);
        IValueBuilder AsDivisor(double dividend);

        // to how many digits depends on the number of significant digits the value has
        // they also need to be rounded to more digits before floored/ceiled to avoid e.g. 0.99999 being floored to 0
        IValueBuilder Rounded { get; }
        IValueBuilder Floored { get; }
        IValueBuilder Ceiled { get; }
    }
}