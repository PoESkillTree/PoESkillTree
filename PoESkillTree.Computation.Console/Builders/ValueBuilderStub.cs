using System;
using System.Globalization;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ValueBuilderStub : BuilderStub, IValueBuilder
    {
        private readonly Resolver<IValueBuilder> _resolver;

        public ValueBuilderStub(string stringRepresentation, Resolver<IValueBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IValueBuilder This => this;

        public IConditionBuilder Eq(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} == {r})");

        public IConditionBuilder Eq(double other) =>
            CreateCondition(This, o => $"({o} == {other})");

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} > {r})");

        public IConditionBuilder GreaterThan(double other) =>
            CreateCondition(This, o => $"({o} > {other})");

        public IValueBuilder Add(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} + {r})");

        public IValueBuilder Add(double other) =>
            CreateValue(This, o => $"({o} + {other})");

        public IValueBuilder Multiply(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} * {r})");

        public IValueBuilder Multiply(double other) =>
            CreateValue(This, o => $"({o} * {other})");

        public IValueBuilder AsDividend(IValueBuilder divisor) =>
            CreateValue(This, divisor, (l, r) => $"({l} / {r})");

        public IValueBuilder AsDividend(double divisor) =>
            CreateValue(This, o => $"({o} / {divisor})");

        public IValueBuilder AsDivisor(double dividend) =>
            CreateValue(This, o => $"({dividend} / {o})");

        public IValueBuilder Rounded => CreateValue(This, o => $"Round({o})");
        public IValueBuilder Floored => CreateValue(This, o => $"Floor({o})");
        public IValueBuilder Ceiled => CreateValue(This, o => $"Ceil({o})");

        public IValueBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class ValueBuildersStub : IValueBuilders
    {
        public IThenBuilder If(IConditionBuilder condition)
        {
            IThenBuilder Resolve(ResolveContext context) =>
                new ThenBuilder($"if ({condition.Resolve(context)})", (current, _) => current);

            return new ThenBuilder($"if ({condition})", (_, context) => Resolve(context));
        }

        public IValueBuilder Create(double value) =>
            CreateValue(value.ToString(CultureInfo.InvariantCulture));

        public Func<IValueBuilder, IValueBuilder> WrapValueConverter(
            Func<ValueBuilder, ValueBuilder> converter)
        {
            return iValue => iValue is ValueBuilder value
                ? converter(value)
                : converter(new ValueBuilder(iValue));
        }


        private class ThenBuilder : BuilderStub, IThenBuilder
        {
            private readonly Resolver<IThenBuilder> _resolver;

            public ThenBuilder(string stringRepresentation, Resolver<IThenBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            public IConditionalValueBuilder Then(IValueBuilder value)
            {
                IConditionalValueBuilder Resolve(ResolveContext context)
                {
                    return new ConditionalValueBuilder(
                        $"{this.Resolve(context)} else if ({value.Resolve(context)})",
                        (current, _) => current);
                }

                return new ConditionalValueBuilder($"{this} else if ({value})",
                    (_, context) => Resolve(context));
            }

            public IConditionalValueBuilder Then(double value)
            {
                IConditionalValueBuilder Resolve(ResolveContext context)
                {
                    return new ConditionalValueBuilder(
                        $"{this.Resolve(context)} else if ({value})",
                        (current, _) => current);
                }

                return new ConditionalValueBuilder($"{this} else if ({value})",
                    (_, context) => Resolve(context));
            }

            public IThenBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }


        private class ConditionalValueBuilder : BuilderStub, IConditionalValueBuilder
        {
            private readonly Resolver<IConditionalValueBuilder> _resolver;

            public ConditionalValueBuilder(
                string stringRepresentation,
                Resolver<IConditionalValueBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            private IConditionalValueBuilder This => this;

            public IThenBuilder ElseIf(IConditionBuilder condition)
            {
                IThenBuilder Resolve(ResolveContext context)
                {
                    return new ThenBuilder(
                        $"{this.Resolve(context)} else if ({condition.Resolve(context)})",
                        (current, _) => current);
                }

                return new ThenBuilder($"{this} else if ({condition})",
                    (_, context) => Resolve(context));
            }

            public ValueBuilder Else(IValueBuilder value) =>
                new ValueBuilder(CreateValue(This, value, (l, r) => $"{l} else {{ {r} }}"));

            public ValueBuilder Else(double value) =>
                new ValueBuilder(CreateValue(This, o => $"{o} else {{ {value} }}"));

            public IConditionalValueBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }
    }
}