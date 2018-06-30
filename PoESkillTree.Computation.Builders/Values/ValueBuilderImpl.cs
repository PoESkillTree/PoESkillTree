using System;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Values
{
    // ("Impl" suffix to avoid confusion with ValueBuilder in Common)
    public class ValueBuilderImpl : IValueBuilder
    {
        private readonly Func<BuildParameters, IValue> _buildValue;
        private readonly Func<ResolveContext, IValueBuilder> _resolve;

        public ValueBuilderImpl(double? value) : this(new Constant(value))
        {
        }

        public ValueBuilderImpl(IValue value) : this(_ => value)
        {
        }

        private ValueBuilderImpl(Func<BuildParameters, IValue> buildValue)
        {
            _buildValue = buildValue;
            _resolve = _ => this;
        }

        public ValueBuilderImpl(
            Func<BuildParameters, IValue> buildValue, Func<ResolveContext, Func<BuildParameters, IValue>> resolve)
            : this(buildValue, c => new ValueBuilderImpl(resolve(c)))
        {
        }

        public ValueBuilderImpl(Func<BuildParameters, IValue> buildValue, Func<ResolveContext, IValueBuilder> resolve)
        {
            _buildValue = buildValue;
            _resolve = resolve;
        }

        public IValueBuilder Resolve(ResolveContext context) => _resolve(context);

        public IValueBuilder MaximumOnly =>
            Create(this, o => o.Select(v => new NodeValue(0, v.Maximum)), o => $"{o}.MaximumOnly");

        public IConditionBuilder Eq(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right) => left == right, (l, r) => $"{l} == {r}");

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right) => left > right, (l, r) => $"{l} > {r}");

        public IValueBuilder Add(IValueBuilder other) =>
            Create(this, other, (left, right) => new[] { left, right }.Sum(), (l, r) => $"{l} + {r}");

        public IValueBuilder Multiply(IValueBuilder other) =>
            Create(this, other, (left, right) => new[] { left, right }.Product(), (l, r) => $"{l} * {r}");

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            Create(this, divisor, (left, right) => left / (right ?? new NodeValue(1)), (l, r) => $"{l} / {r}");

        public IValueBuilder If(IValue condition) =>
            Create(this, new ValueBuilderImpl(condition), (l, r) => r.IsTrue() ? l : null,
                (l, r) => $"{r}.IsTrue ? {l} : null");

        public IValueBuilder Select(Func<double, double> selector, Func<IValue, string> identity) =>
            Create(this, o => o.Select(selector), identity);

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValue Build(BuildParameters parameters) => _buildValue(parameters);

        // TODO Only here for compatibility with stubs in Console. Remove once those are removed.
        public override string ToString() => Build(default).ToString();


        public static IValueBuilder Create(
            IValueBuilder operand,
            Func<NodeValue?, NodeValue?> calculate,
            Func<IValue, string> identity) =>
            new ValueBuilderImpl(
                ps => Build(ps, operand, calculate, identity),
                c => (ps => Build(ps, operand.Resolve(c), calculate, identity)));

        public static IValueBuilder Create(
            IValueBuilder left, IValueBuilder right,
            Func<NodeValue?, NodeValue?, NodeValue?> calculate,
            Func<IValue, IValue, string> identity) =>
            new ValueBuilderImpl(
                ps => Build(ps, left, right, calculate, identity),
                c => (ps => Build(ps, left.Resolve(c), right.Resolve(c), calculate, identity)));

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder operand,
            Func<NodeValue?, NodeValue?> calculate,
            Func<IValue, string> identity)
        {
            var builtOperand = operand.Build(parameters);
            return new FunctionalValue(c => calculate(builtOperand.Calculate(c)), identity(builtOperand));
        }

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder left, IValueBuilder right,
            Func<NodeValue?, NodeValue?, NodeValue?> calculate,
            Func<IValue, IValue, string> identity)
        {
            var l = left.Build(parameters);
            var r = right.Build(parameters);
            return new FunctionalValue(c => calculate(l.Calculate(c), r.Calculate(c)), identity(l, r));
        }
    }
}