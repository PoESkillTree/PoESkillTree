using System;
using System.Linq.Expressions;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public static class ValueConditionBuilder
    {
        public static IConditionBuilder Create(IValueBuilder operand, Expression<Func<NodeValue?, bool>> calculate) =>
            new ValueConditionBuilder<IValueBuilder>((p, o) => Build(p, o, calculate), operand);

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder operand,
            Expression<Func<NodeValue?, bool>> calculate)
        {
            var builtOperand = operand.Build(parameters);
            var func = calculate.Compile();
            var identity = calculate.ToString(builtOperand);
            return new ConditionalValue(c => func(builtOperand.Calculate(c)), identity);
        }

        public static IConditionBuilder Create(
            IValueBuilder operand1, IValueBuilder operand2,
            Expression<Func<NodeValue?, NodeValue?, bool>> calculate) =>
            new ValueConditionBuilder<IValueBuilder, IValueBuilder>(
                (p, o1, o2) => Build(p, o1, o2, calculate), operand1, operand2);

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder operand1, IValueBuilder operand2,
            Expression<Func<NodeValue?, NodeValue?, bool>> calculate)
        {
            var o1 = operand1.Build(parameters);
            var o2 = operand2.Build(parameters);
            var func = calculate.Compile();
            var identity = calculate.ToString(o1, o2);
            return new ConditionalValue(c => func(o1.Calculate(c), o2.Calculate(c)), identity);
        }

        public static IConditionBuilder Create<TParameter>(
            Func<BuildParameters, TParameter, IStat> buildStat, TParameter parameter)
            where TParameter : IResolvable<TParameter>
        {
            return new ValueConditionBuilder<TParameter>((p, t) => new StatValue(buildStat(p, t)), parameter);
        }
    }

    public class ValueConditionBuilder<TParameter> : ConditionBuilderBase
        where TParameter : IResolvable<TParameter>
    {
        private readonly Func<BuildParameters, TParameter, IValue> _buildValue;
        private readonly TParameter _parameter;

        public ValueConditionBuilder(Func<BuildParameters, TParameter, IValue> buildValue, TParameter parameter)
        {
            _buildValue = buildValue;
            _parameter = parameter;
        }

        public override IConditionBuilder Resolve(ResolveContext context) =>
            new ValueConditionBuilder<TParameter>(_buildValue, _parameter.Resolve(context));

        public override IConditionBuilder Not =>
            new ValueConditionBuilder<TParameter>((b, p) => new NotValue(_buildValue(b, p)), _parameter);

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(_buildValue(parameters, _parameter));

        // TODO Only here for compatibility with stubs in Console. Remove once those are removed.
        public override string ToString() => Build(default).Value.ToString();
    }

    public class ValueConditionBuilder<TParameter1, TParameter2> : ConditionBuilderBase
        where TParameter1 : IResolvable<TParameter1>
        where TParameter2 : IResolvable<TParameter2>
    {
        private readonly Func<BuildParameters, TParameter1, TParameter2, IValue> _buildValue;
        private readonly TParameter1 _parameter1;
        private readonly TParameter2 _parameter2;

        public ValueConditionBuilder(
            Func<BuildParameters, TParameter1, TParameter2, IValue> buildValue,
            TParameter1 parameter1, TParameter2 parameter2)
        {
            _buildValue = buildValue;
            _parameter1 = parameter1;
            _parameter2 = parameter2;
        }

        public override IConditionBuilder Resolve(ResolveContext context) =>
            new ValueConditionBuilder<TParameter1, TParameter2>(
                _buildValue, _parameter1.Resolve(context), _parameter2.Resolve(context));

        public override IConditionBuilder Not =>
            new ValueConditionBuilder<TParameter1, TParameter2>(
                (b, p1, p2) => new NotValue(_buildValue(b, p1, p2)), _parameter1, _parameter2);

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(_buildValue(parameters, _parameter1, _parameter2));

        // TODO Only here for compatibility with stubs in Console. Remove once those are removed.
        public override string ToString() => Build(default).Value.ToString();
    }
}