namespace PoESkillTree.Computation.Parsing.Builders.Forms
{
    public interface IFormBuilders
    {
        // Sets the initial base value. Can only be set once.
        IFormBuilder BaseSet { get; }

        IFormBuilder PercentIncrease { get; }
        IFormBuilder PercentMore { get; }
        IFormBuilder BaseAdd { get; }

        // These three apply to the three above but with value * -1
        IFormBuilder PercentReduce { get; }
        IFormBuilder PercentLess { get; }
        IFormBuilder BaseSubtract { get; }

        // Sets the total value and discards all other values
        // For damage stats, this applies after conversion
        // The value may go above stat.Maximum or below stat.Minimum
        IFormBuilder TotalOverride { get; }

        // BaseAdd for minimum and maximum of damage stats
        IFormBuilder MinBaseAdd { get; }
        IFormBuilder MaxBaseAdd { get; }

        // BaseAdd for stat.Maximum
        IFormBuilder MaximumAdd { get; }

        // Shortcut for TotalOverride with value 1
        IFormBuilder SetFlag { get; }
        // Shortcuts for TotalOverride with value 0
        IFormBuilder Zero { get; }
        // Shortcut for TotalOverride with value 100
        IFormBuilder Always { get; }
    }
}