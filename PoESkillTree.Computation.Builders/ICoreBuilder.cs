using System;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders
{
    public interface ICoreBuilder<out TResult> : IResolvable<ICoreBuilder<TResult>>
    {
        TResult Build();
    }

    // As opposed to the constructors they call, these methods allow generic type inference.
    public static class CoreBuilder
    {
        public static ICoreBuilder<TResult> Create<TResult>(TResult result) =>
            new ConstantCoreBuilder<TResult>(result);

        public static ICoreBuilder<TResult>
            Create<TParameter, TResult>(TParameter parameter, Func<TParameter, TResult> build)
            where TParameter : IResolvable<TParameter>
        {
            return new ParametrisedCoreBuilder<TParameter, TResult>(parameter, build);
        }

        public static ICoreBuilder<TResult> UnaryOperation<TResult>(
            ICoreBuilder<TResult> operand, Func<TResult, TResult> @operator)
        {
            return new UnaryOperatorCoreBuilder<TResult>(operand, @operator);
        }

        public static ICoreBuilder<TResult> BinaryOperation<TResult>(
            ICoreBuilder<TResult> left, ICoreBuilder<TResult> right, Func<TResult, TResult, TResult> @operator)
        {
            return new BinaryOperatorCoreBuilder<TResult>(left, right, @operator);
        }
    }

    internal class ConstantCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly TResult _result;

        public ConstantCoreBuilder(TResult result) => _result = result;

        public ICoreBuilder<TResult> Resolve(ResolveContext context) => this;

        public TResult Build() => _result;
    }

    internal class ParametrisedCoreBuilder<TParameter, TResult> : ICoreBuilder<TResult>
        where TParameter : IResolvable<TParameter>
    {
        private readonly TParameter _parameter;
        private readonly Func<TParameter, TResult> _build;

        public ParametrisedCoreBuilder(TParameter parameter, Func<TParameter, TResult> build)
        {
            _parameter = parameter;
            _build = build;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new ParametrisedCoreBuilder<TParameter, TResult>(_parameter.Resolve(context), _build);

        public TResult Build() => _build(_parameter);
    }

    internal class UnaryOperatorCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly ICoreBuilder<TResult> _operand;
        private readonly Func<TResult, TResult> _operator;

        public UnaryOperatorCoreBuilder(ICoreBuilder<TResult> operand, Func<TResult, TResult> @operator)
        {
            _operand = operand;
            _operator = @operator;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new UnaryOperatorCoreBuilder<TResult>(_operand.Resolve(context), _operator);

        public TResult Build() =>
            _operator(_operand.Build());
    }

    internal class BinaryOperatorCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly ICoreBuilder<TResult> _left;
        private readonly ICoreBuilder<TResult> _right;
        private readonly Func<TResult, TResult, TResult> _operator;

        public BinaryOperatorCoreBuilder(
            ICoreBuilder<TResult> left, ICoreBuilder<TResult> right, Func<TResult, TResult, TResult> @operator)
        {
            _left = left;
            _right = right;
            _operator = @operator;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new BinaryOperatorCoreBuilder<TResult>(_left.Resolve(context), _right.Resolve(context), _operator);

        public TResult Build() =>
            _operator(_left.Build(), _right.Build());
    }
}