namespace PoESkillTree.Computation.Providers.Forms
{
    public interface IFormProviderFactory
    {
        // Sets the initial base value. Can only be set once.
        IFormProvider BaseSet { get; }

        IFormProvider PercentIncrease { get; }
        IFormProvider PercentMore { get; }
        IFormProvider BaseAdd { get; }

        // These three apply to the three above but with value * -1
        IFormProvider PercentReduce { get; }
        IFormProvider PercentLess { get; }
        IFormProvider BaseSubtract { get; }

        // Sets the total value and discards all other values
        // For damage stats, this applies after conversion
        // The value may go above stat.Maximum or below stat.Minimum
        IFormProvider TotalOverride { get; }

        // BaseAdd for minimum and maximum of damage stats
        IFormProvider MinBaseAdd { get; }
        IFormProvider MaxBaseAdd { get; }

        // BaseAdd for stat.Maximum
        IFormProvider MaximumAdd { get; }

        // Shortcut for TotalOverride with value 1
        IFormProvider SetFlag { get; }
        // Shortcuts for TotalOverride with value 0
        IFormProvider Zero { get; }
        // Shortcut for TotalOverride with value 100
        IFormProvider Always { get; }
    }
}