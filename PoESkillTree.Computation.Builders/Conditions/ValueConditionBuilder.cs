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
    public class ValueConditionBuilder : ConditionBuilderBase
    {
        private readonly Func<BuildParameters, IValue> _buildValue;
        private readonly Func<ResolveContext, IConditionBuilder> _resolve;

        public ValueConditionBuilder(bool value) : this(_ => new Constant(value))
        {
        }

        private ValueConditionBuilder(Func<BuildParameters, IValue> buildValue)
        {
            _buildValue = buildValue;
            _resolve = _ => this;
        }

        private ValueConditionBuilder(
            Func<BuildParameters, IValue> buildValue, Func<ResolveContext, Func<BuildParameters, IValue>> resolve)
        {
            _buildValue = buildValue;
            _resolve = c => new ValueConditionBuilder(resolve(c));
        }

        public override IConditionBuilder Resolve(ResolveContext context) => _resolve(context);

        public override IConditionBuilder Not => Create(this, b => !b);

        public override (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (Funcs.Identity, _buildValue(parameters));

        // TODO Only here for compatibility with stubs in Console. Remove once those are removed.
        public override string ToString() => Build(default).value.ToString();


        private static IConditionBuilder Create(
            ValueConditionBuilder operand,
            Expression<Func<bool, bool>> calculate) =>
            new ValueConditionBuilder(
                ps => Build(ps, operand, calculate),
                c => (ps => Build(ps, operand.Resolve(c), calculate)));

        public static IConditionBuilder Create(
            IValueBuilder operand,
            Expression<Func<NodeValue?, bool>> calculate) =>
            new ValueConditionBuilder(
                ps => Build(ps, operand, calculate),
                c => (ps => Build(ps, operand.Resolve(c), calculate)));

        public static IConditionBuilder Create(
            IValueBuilder operand1, IValueBuilder operand2,
            Expression<Func<NodeValue?, NodeValue?, bool>> calculate) =>
            new ValueConditionBuilder(
                ps => Build(ps, operand1, operand2, calculate),
                c => (ps => Build(ps, operand1.Resolve(c), operand2.Resolve(c), calculate)));

        private static IValue Build(
            BuildParameters parameters,
            IConditionBuilder operand,
            Expression<Func<bool, bool>> calculate)
        {
            var builtOperand = operand.Build(parameters).value;
            var func = calculate.Compile();
            var identity = calculate.ToString(builtOperand);
            return new ConditionalValue(c => func(builtOperand.Calculate(c).IsTrue()), identity);
        }

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
            new ValueConditionBuilder<TParameter>((b, p) => NotValue(_buildValue(b, p)), _parameter);

        private static IValue NotValue(IValue value) =>
            new ConditionalValue(c => !value.Calculate(c).IsTrue(), $"Not({value})");

        public override (StatConverter statConverter, IValue value) Build(BuildParameters parameters) =>
            (Funcs.Identity, _buildValue(parameters, _parameter));
    }
}