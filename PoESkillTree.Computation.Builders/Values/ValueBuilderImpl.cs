using System;
using System.Linq.Expressions;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Values
{
    // ("Impl" suffix to avoid confusion with ValueBuilder in Common)
    public class ValueBuilderImpl : IValueBuilder
    {
        private readonly Func<IValue> _buildValue;
        private readonly Func<ResolveContext, IValueBuilder> _resolve;

        public ValueBuilderImpl(double? value) : this(new Constant(value))
        {
        }

        public ValueBuilderImpl(IValue value) : this(() => value)
        {
        }

        private ValueBuilderImpl(Func<IValue> buildValue)
        {
            _buildValue = buildValue;
            _resolve = _ => this;
        }

        public ValueBuilderImpl(Func<IValue> buildValue, Func<ResolveContext, Func<IValue>> resolve)
        {
            _buildValue = buildValue;
            _resolve = c => new ValueBuilderImpl(resolve(c));
        }

        protected ValueBuilderImpl(Func<IValue> buildValue, Func<ResolveContext, IValueBuilder> resolve)
        {
            _buildValue = buildValue;
            _resolve = resolve;
        }

        public IValueBuilder Resolve(ResolveContext context) => _resolve(context);

        public IValueBuilder MaximumOnly =>
            Create(this, o => o.Select(v => new NodeValue(0, v.Maximum)), o => $"{o}.MaximumOnly");

        public IConditionBuilder Eq(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right) => left == right);

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right) => left > right);

        public IValueBuilder Add(IValueBuilder other) =>
            Create(this, other, (left, right) => new[] { left, right }.Sum(), (l, r) => $"{l} + {r}");

        public IValueBuilder Multiply(IValueBuilder other) =>
            Create(this, other, (left, right) => new[] { left, right }.Product(), (l, r) => $"{l} * {r}");

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            Create(this, divisor, (left, right) => left / (right ?? new NodeValue(1)));

        public IValueBuilder Select(Expression<Func<double, double>> selector) =>
            Create(this, o => o.Select(selector.Compile()), o => selector.ToString(o));

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValue Build() => _buildValue();

        public override string ToString() => Build().ToString();


        public static IValueBuilder Create(
            IValueBuilder operand,
            Expression<Func<NodeValue?, NodeValue?>> calculate,
            Func<IValue, string> identityOverride = null) =>
            new ValueBuilderImpl(
                () => Build(operand, calculate, identityOverride),
                c => (() => Build(operand.Resolve(c), calculate, identityOverride)));

        public static IValueBuilder Create(
            IValueBuilder operand1, IValueBuilder operand2,
            Expression<Func<NodeValue?, NodeValue?, NodeValue?>> calculate,
            Func<IValue, IValue, string> identityOverride = null) =>
            new ValueBuilderImpl(
                () => Build(operand1, operand2, calculate, identityOverride),
                c => (() => Build(operand1.Resolve(c), operand2.Resolve(c), calculate, identityOverride)));

        private static IValue Build(
            IValueBuilder operand,
            Expression<Func<NodeValue?, NodeValue?>> calculate,
            Func<IValue, string> identityOverride = null)
        {
            var builtOperand = operand.Build();
            var func = calculate.Compile();
            var identity = identityOverride is null ? calculate.ToString(builtOperand) : identityOverride(builtOperand);
            return new FunctionalValue(c => func(builtOperand.Calculate(c)), identity);
        }

        private static IValue Build(
            IValueBuilder operand1, IValueBuilder operand2,
            Expression<Func<NodeValue?, NodeValue?, NodeValue?>> calculate,
            Func<IValue, IValue, string> identityOverride = null)
        {
            var o1 = operand1.Build();
            var o2 = operand2.Build();
            var func = calculate.Compile();
            var identity = identityOverride is null ? calculate.ToString(o1, o2) : identityOverride(o1, o2);
            return new FunctionalValue(c => func(o1.Calculate(c), o2.Calculate(c)), identity);
        }
    }
}