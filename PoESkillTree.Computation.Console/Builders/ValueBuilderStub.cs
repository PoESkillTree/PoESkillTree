using System;
using System.Globalization;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
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

        public IValueBuilder MaximumOnly =>
            CreateValue(This, o => $"{o} (maximum value only)");

        public IConditionBuilder Eq(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} == {r})");

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            CreateCondition(This, other, (l, r) => $"({l} > {r})");

        public IValueBuilder Add(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} + {r})");

        public IValueBuilder Multiply(IValueBuilder other) =>
            CreateValue(This, other, (l, r) => $"({l} * {r})");

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            CreateValue(This, divisor, (l, r) => $"({l} / {r})");

        public IValueBuilder Select(Func<double, double> selector) => 
            CreateValue(This, o => $"{selector}({o})");

        public IValueBuilder Create(double value) => 
            new ValueBuilderStub(value.ToString(CultureInfo.InvariantCulture), (c, _) => c);

        public IValueBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);

        public IValue Build() => new ValueStub(this);


        private class ValueStub : BuilderStub, IValue
        {
            public ValueStub(BuilderStub builderStub) : base(builderStub)
            {
            }

            public NodeValue? Calculate(IValueCalculationContext valueCalculationContext)
            {
                throw new NotImplementedException();
            }
        }
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

        public IValueBuilder FromMinAndMax(IValueBuilder minimumValue, IValueBuilder maximumValue) => 
            CreateValue(minimumValue, maximumValue, (o1, o2) => $"Min: {o1}, Max: {o2}");


        private class ThenBuilder : BuilderStub, IThenBuilder, IResolvable<IThenBuilder>
        {
            private readonly Resolver<IThenBuilder> _resolver;

            public ThenBuilder(string stringRepresentation, Resolver<IThenBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            public IConditionalValueBuilder Then(IValueBuilder value)
            {
                ConditionalValueBuilder Resolve(ResolveContext context)
                {
                    return new ConditionalValueBuilder(
                        $"{this.Resolve(context)} else if ({value.Resolve(context)})",
                        (current, _) => current);
                }

                return new ConditionalValueBuilder($"{this} else if ({value})", (_, context) => Resolve(context));
            }

            public IConditionalValueBuilder Then(double value)
            {
                ConditionalValueBuilder Resolve(ResolveContext context)
                {
                    return new ConditionalValueBuilder(
                        $"{this.Resolve(context)} else if ({value})",
                        (current, _) => current);
                }

                return new ConditionalValueBuilder($"{this} else if ({value})", (_, context) => Resolve(context));
            }

            public IThenBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }


        private class ConditionalValueBuilder : BuilderStub, IConditionalValueBuilder, IResolvable<ConditionalValueBuilder>
        {
            private readonly Resolver<ConditionalValueBuilder> _resolver;

            public ConditionalValueBuilder(string stringRepresentation, Resolver<ConditionalValueBuilder> resolver)
                : base(stringRepresentation)
            {
                _resolver = resolver;
            }

            private ConditionalValueBuilder This => this;

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

            public ConditionalValueBuilder Resolve(ResolveContext context) =>
                _resolver(this, context);
        }
    }
}