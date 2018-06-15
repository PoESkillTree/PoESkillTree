using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary>
    /// Behavior of Source.ConvertTo(Target) and GainAs(Target).
    /// Applies to Target.PathTotal (conversion paths only).
    /// Returns value * (ConvertTo + GainAs).
    /// </summary>
    public class ConversionTargetPathTotalValue : IValue
    {
        private readonly IStat _convertTo;
        private readonly IStat _gainAs;
        private readonly IValue _transformedValue;

        public ConversionTargetPathTotalValue(IStat convertTo, IStat gainAs, IValue transformedValue)
        {
            _convertTo = convertTo;
            _gainAs = gainAs;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            if (value is null)
                return null;

            var conversion = context.GetValue(_convertTo) ?? new NodeValue(0);
            var gain = context.GetValue(_gainAs) ?? new NodeValue(0);
            return value * (conversion + gain) / 100;
        }
    }
}