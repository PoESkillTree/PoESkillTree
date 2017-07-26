using System;

namespace PoESkillTree.Computation.Providers
{
    public interface IFormProvider
    {

    }

    public static class FormProviders
    {
        // Sets the initial base value. Can only be set once.
        public static readonly IFormProvider BaseSet;
        public static readonly IFormProvider PercentIncrease;
        public static readonly IFormProvider PercentMore;
        public static readonly IFormProvider BaseAdd;
        // These three apply to the three above but with value * -1
        public static readonly IFormProvider PercentReduction;
        public static readonly IFormProvider PercentLess;
        public static readonly IFormProvider BaseSubtract;
        // Sets the total value and discards all other values
        // For damage stats, this applies after conversion
        // The value may go above stat.Maximum or below stat.Minimum
        public static readonly IFormProvider TotalOverride;
        // BaseAdd for minimum and maximum of damage stats
        public static readonly IFormProvider MinBaseAdd;
        public static readonly IFormProvider MaxBaseAdd;
        // BaseAdd for stat.Maximum
        public static readonly IFormProvider MaximumAdd;

        // shortcut for TotalOverride with value 1
        public static readonly IFormProvider SetFlag;
        // shortcuts for TotalOverride with value 0
        public static readonly IFormProvider Zero;
        public static readonly IFormProvider Never;
        // shortcut for TotalOverride with value 100
        public static readonly IFormProvider Always;

        public static IFormProvider MultiValued(params IFormProvider[] formPerValueIndex)
        {
            throw new NotImplementedException();
        }
    }
}