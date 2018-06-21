using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Values
{
    public class ValueBuilders : IValueBuilders
    {
        public IThenBuilder If(IConditionBuilder condition) => new ThenBuilder(condition);

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValueBuilder FromMinAndMax(IValueBuilder minimumValue, IValueBuilder maximumValue) =>
            ValueBuilderImpl.Create(minimumValue, maximumValue, (o1, o2) => CalculateFromMinAndMax(o1, o2));

        private static NodeValue? CalculateFromMinAndMax(NodeValue? min, NodeValue? max) =>
            min.HasValue && max.HasValue
                ? new NodeValue(min.Value.Minimum, max.Value.Maximum)
                : (NodeValue?) null;


        private class ThenBuilder : IThenBuilder
        {
            private readonly IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> _conditionValuePairs;
            private readonly IConditionBuilder _branchCondition;

            public ThenBuilder(IConditionBuilder branchCondition)
            {
                _conditionValuePairs = new (IConditionBuilder condition, IValueBuilder value)[0];
                _branchCondition = branchCondition;
            }

            public ThenBuilder(
                IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> conditionValuePairs,
                IConditionBuilder branchCondition)
            {
                _conditionValuePairs = conditionValuePairs;
                _branchCondition = branchCondition;
            }

            public IConditionalValueBuilder Then(IValueBuilder value) =>
                new ConditionalValueBuilder(_conditionValuePairs.Append((_branchCondition, value)).ToList());

            public IConditionalValueBuilder Then(double value) => Then(new ValueBuilderImpl(value));
        }


        private class ConditionalValueBuilder : IConditionalValueBuilder
        {
            private readonly IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> _conditionValuePairs;

            public ConditionalValueBuilder(
                IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> conditionValuePairs)
            {
                _conditionValuePairs = conditionValuePairs;
            }

            public IThenBuilder ElseIf(IConditionBuilder condition) =>
                new ThenBuilder(_conditionValuePairs, condition);

            public ValueBuilder Else(IValueBuilder value)
            {
                var valueBuilder = new ValueBuilderImpl(
                    ps => Build(ps, _conditionValuePairs, value),
                    c => (ps => Build(ps, Resolve(c, _conditionValuePairs), value.Resolve(c))));
                return new ValueBuilder(valueBuilder);

                IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> Resolve(
                    ResolveContext context,
                    IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> conditionValuePairs) =>
                    (from t in conditionValuePairs
                     select (t.condition.Resolve(context), t.value.Resolve(context))
                    ).ToList();

                IValue Build(
                    BuildParameters parameters,
                    IReadOnlyList<(IConditionBuilder condition, IValueBuilder value)> conditionValuePairs,
                    IValueBuilder elseValue)
                {
                    var pairs = new List<(IValue condition, IValue value)>();
                    foreach (var (c, v) in conditionValuePairs)
                    {
                        var condition = c.Build(parameters);
                        if (condition.HasStatConverter)
                        {
                            throw new ParseException(
                                $"Conditions for building conditional values must be value conditions. {c}");
                        }
                        pairs.Add((condition.Value, v.Build(parameters)));
                    }
                    return new BranchingValue(pairs, elseValue.Build(parameters));
                }
            }

            public ValueBuilder Else(double value) => Else(new ValueBuilderImpl(0));
        }


        private class BranchingValue : IValue
        {
            private readonly IReadOnlyList<(IValue condition, IValue value)> _conditionValuePairs;
            private readonly IValue _elseValue;

            public BranchingValue(
                IReadOnlyList<(IValue condition, IValue value)> conditionValuePairs, IValue elseValue)
            {
                _conditionValuePairs = conditionValuePairs;
                _elseValue = elseValue;
            }

            public NodeValue? Calculate(IValueCalculationContext context)
            {
                foreach (var (c, v) in _conditionValuePairs)
                {
                    if (c.Calculate(context).IsTrue())
                    {
                        return v.Calculate(context);
                    }
                }
                return _elseValue.Calculate(context);
            }

            public override string ToString()
            {
                var s = $"If ({_conditionValuePairs[0].condition}): {_conditionValuePairs[0].value}\n";
                foreach (var (c, v) in _conditionValuePairs.Skip(1))
                {
                    s += $"Else If ({c}): {v}\n";
                }
                return s + $"Else: {_elseValue}";
            }
        }
    }
}