namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Represents a value calculation that can be evaluated.
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// Calculates and returns the current value using the given context.
        /// </summary>
        NodeValue? Calculate(IValueCalculationContext context);
    }
}