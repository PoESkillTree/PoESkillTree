using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class ValueConditionBuilder : ConditionBuilderBase
    {
        private readonly Func<BuildParameters, IValue> _buildValue;
        private readonly Func<ResolveContext, IConditionBuilder> _resolver;

        public ValueConditionBuilder(Func<BuildParameters, IValue> buildValue)
        {
            _buildValue = buildValue;
            _resolver = _ => this;
        }

        public ValueConditionBuilder(
            Func<BuildParameters, IValue> buildValue, Func<ResolveContext, IConditionBuilder> resolver)
        {
            _buildValue = buildValue;
            _resolver = resolver;
        }

        public override IConditionBuilder Resolve(ResolveContext context) => _resolver(context);

        public override IConditionBuilder Not =>
            new ValueConditionBuilder(ps => new NotValue(_buildValue(ps)), _resolver.AndThen(b => b.Not));

        public override ConditionBuilderResult Build(BuildParameters parameters) =>
            new ConditionBuilderResult(_buildValue(parameters));

        public static IConditionBuilder Create(
            IValueBuilder operand,
            Func<NodeValue?, bool> calculate,
            Func<IValue, string> identity) =>
            new ValueConditionBuilder<IValueBuilder>((p, o) => Build(p, o, calculate, identity), operand);

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder operand,
            Func<NodeValue?, bool> calculate,
            Func<IValue, string> identity)
        {
            var builtOperand = operand.Build(parameters);
            return new ConditionalValue(c => calculate(builtOperand.Calculate(c)), identity(builtOperand));
        }

        public static IConditionBuilder Create(
            IValueBuilder left, IValueBuilder right,
            Func<NodeValue?, NodeValue?, bool> calculate,
            Func<IValue, IValue, string> identity) =>
            new ValueConditionBuilder<IValueBuilder, IValueBuilder>(
                (p, o1, o2) => Build(p, o1, o2, calculate, identity), left, right);

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder left, IValueBuilder right,
            Func<NodeValue?, NodeValue?, bool> calculate,
            Func<IValue, IValue, string> identity)
        {
            var l = left.Build(parameters);
            var r = right.Build(parameters);
            return new ConditionalValue(c => calculate(l.Calculate(c), r.Calculate(c)), identity(l, r));
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
    }
}