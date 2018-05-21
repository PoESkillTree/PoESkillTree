using System;

namespace PoESkillTree.Computation.Common
{
    public class Constant : IValue
    {
        private readonly NodeValue? _value;

        public Constant(double? value) =>
            _value = (NodeValue?) value;

        public Constant(NodeValue? value) =>
            _value = value;

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
            _value;
    }


    public class FunctionalValue : IValue
    {
        private readonly Func<IValueCalculationContext, NodeValue?> _calculate;

        public FunctionalValue(Func<IValueCalculationContext, NodeValue?> calculate) =>
            _calculate = calculate;

        public NodeValue? Calculate(IValueCalculationContext context) =>
            _calculate(context);
    }


    public class ConditionalValue : IValue
    {
        private readonly Predicate<IValueCalculationContext> _calculate;

        public ConditionalValue(Predicate<IValueCalculationContext> calculate) =>
            _calculate = calculate;

        public NodeValue? Calculate(IValueCalculationContext context) =>
            (NodeValue?) _calculate(context);
    }
}