using System;
using System.Globalization;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ValueBuilderStub : BuilderStub, IValueBuilder
    {
        public ValueBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IConditionBuilder Eq(IValueBuilder other) => 
            new ConditionBuilderStub($"({this} == {other})");

        public IConditionBuilder Eq(double other) => 
            new ConditionBuilderStub($"({this} == {other})");

        public IConditionBuilder GreaterThen(IValueBuilder other) => 
            new ConditionBuilderStub($"({this} > {other})");

        public IConditionBuilder GreaterThen(double other) => 
            new ConditionBuilderStub($"({this} > {other})");

        public IValueBuilder Add(IValueBuilder other) => 
            new ValueBuilderStub($"({this} + {other})");

        public IValueBuilder Add(double other) => 
            new ValueBuilderStub($"({this} + {other})");

        public IValueBuilder Multiply(IValueBuilder other) => 
            new ValueBuilderStub($"({this} * {other})");

        public IValueBuilder Multiply(double other) => 
            new ValueBuilderStub($"({this} * {other})");

        public IValueBuilder AsDividend(IValueBuilder divisor) => 
            new ValueBuilderStub($"({this} / {divisor})");

        public IValueBuilder AsDividend(double divisor) => 
            new ValueBuilderStub($"({this} / {divisor})");

        public IValueBuilder AsDivisor(double dividend) => 
            new ValueBuilderStub($"({dividend} / {this})");

        public IValueBuilder Rounded => new ValueBuilderStub($"Round({this})");
        public IValueBuilder Floored => new ValueBuilderStub($"Floor({this})");
        public IValueBuilder Ceiled => new ValueBuilderStub($"Ceil({this})");
    }


    public class ValueBuildersStub : IValueBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public ValueBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IThenBuilder If(IConditionBuilder condition)
        {
            return new ThenBuilder($"if ({condition})", _conditionBuilders);
        }

        public IValueBuilder Create(double value)
        {
            return new ValueBuilderStub(value.ToString(CultureInfo.InvariantCulture));
        }

        public Func<IValueBuilder, IValueBuilder> WrapValueConverter(
            Func<ValueBuilder, ValueBuilder> converter) =>
            iValue => iValue is ValueBuilder value
                ? converter(value)
                : converter(new ValueBuilder(iValue, _conditionBuilders));


        private class ThenBuilder : BuilderStub, IThenBuilder
        {
            private readonly IConditionBuilders _conditionBuilders;

            public ThenBuilder(string stringRepresentation, IConditionBuilders conditionBuilders)
                : base(stringRepresentation)
            {
                _conditionBuilders = conditionBuilders;
            }

            public IConditionalValueBuilder Then(ValueBuilder value)
            {
                return new ConditionalValueBuilder(this + " {" + value + "}", _conditionBuilders);
            }

            public IConditionalValueBuilder Then(double value)
            {
                return new ConditionalValueBuilder(this + " {" + value + "}", _conditionBuilders);
            }
        }


        private class ConditionalValueBuilder : BuilderStub, IConditionalValueBuilder
        {
            private readonly IConditionBuilders _conditionBuilders;

            public ConditionalValueBuilder(string stringRepresentation,
                IConditionBuilders conditionBuilders) : base(stringRepresentation)
            {
                _conditionBuilders = conditionBuilders;
            }

            public IThenBuilder ElseIf(IConditionBuilder condition)
            {
                return new ThenBuilder($"{this} else if ({condition})", _conditionBuilders);
            }

            public ValueBuilder Else(ValueBuilder value)
            {
                return new ValueBuilder(new ValueBuilderStub(this + " else { " + value + " }"),
                    _conditionBuilders);
            }

            public ValueBuilder Else(double value)
            {
                return new ValueBuilder(new ValueBuilderStub(this + " else { " + value + " }"),
                    _conditionBuilders);
            }
        }
    }
}