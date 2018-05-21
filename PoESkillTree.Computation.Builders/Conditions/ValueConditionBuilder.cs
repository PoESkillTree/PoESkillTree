using System;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Conditions
{
    public class ValueConditionBuilder : IConditionBuilder
    {
        private readonly Func<IValue> _buildValue;

        public ValueConditionBuilder(bool value) : this(new ConditionalValue(_ => value))
        {
        }

        public ValueConditionBuilder(IValue value) : this(() => value)
        {
        }

        public ValueConditionBuilder(Func<IValue> buildValue)
        {
            _buildValue = buildValue;
        }

        public IConditionBuilder Resolve(ResolveContext context) => this;

        public IConditionBuilder And(IConditionBuilder condition) =>
            new AndCompositeConditionBuilder(this, condition);

        public IConditionBuilder Or(IConditionBuilder condition) =>
            new OrCompositeConditionBuilder(this, condition);

        public IConditionBuilder Not => Create(this, (o, c) => !o);

        public (StatConverter statConverter, IValue value) Build() =>
            (Funcs.Identity, _buildValue());


        private static IConditionBuilder Create(
            ValueConditionBuilder operand, Func<bool, IValueCalculationContext, bool> calculate) => 
            new ValueConditionBuilder(() => Build(operand, calculate));

        public static IConditionBuilder Create(
            IValueBuilder operand, Func<NodeValue?, IValueCalculationContext, bool> calculate) => 
            new ValueConditionBuilder(() => Build(operand, calculate));

        public static IConditionBuilder Create(
            IValueBuilder operand1, IValueBuilder operand2, 
            Func<NodeValue?, NodeValue?, IValueCalculationContext, bool> calculate) => 
            new ValueConditionBuilder(() => Build(operand1, operand2, calculate));

        private static IValue Build(ValueConditionBuilder operand, Func<bool, IValueCalculationContext, bool> calculate)
        {
            var builtOperand = operand.Build().value;
            return new ConditionalValue(c => calculate(builtOperand.Calculate(c).IsTrue(), c));
        }

        private static IValue Build(IValueBuilder operand, Func<NodeValue?, IValueCalculationContext, bool> calculate)
        {
            var builtOperand = operand.Build();
            return new ConditionalValue(c => calculate(builtOperand.Calculate(c), c));
        }

        private static IValue Build(
            IValueBuilder operand1, IValueBuilder operand2, 
            Func<NodeValue?, NodeValue?, IValueCalculationContext, bool> calculate)
        {
            var o1 = operand1.Build();
            var o2 = operand2.Build();
            return new ConditionalValue(c => calculate(o1.Calculate(c), o2.Calculate(c), c));
        }
    }
}