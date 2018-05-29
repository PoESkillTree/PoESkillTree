namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Defines a transformation applying <see cref="IValue"/> instances.
    /// </summary>
    public interface IValueTransformation
    {
        /// <summary>
        /// Transforms the given value.
        /// </summary>
        /// <remarks>
        /// When implementing this, the returned value will generally either use the result of the given value and
        /// modify it or pass a modified <see cref="IValueCalculationContext"/> to the given value.
        /// </remarks>
        IValue Transform(IValue value);
    }
}