using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    /// <summary>
    /// Behavior of Source.ConvertTo(Target) and GainAs(Target).
    /// Applies to Target.Base (conversion paths only).
    /// Returns value * (ConvertTo + GainAs).
    /// </summary>
    public class ConversionTargetBaseValue : IValue
    {
        public IStat ConvertTo { get; }
        public IStat GainAs { get; }
        private readonly IValue _transformedValue;

        public ConversionTargetBaseValue(IStat convertTo, IStat gainAs, IValue transformedValue)
        {
            ConvertTo = convertTo;
            GainAs = gainAs;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var value = _transformedValue.Calculate(context);
            if (value is null)
                return null;

            var conversion = context.GetValue(ConvertTo) ?? new NodeValue(0);
            var gain = context.GetValue(GainAs) ?? new NodeValue(0);
            return value * (conversion + gain) / 100;
        }
    }
}