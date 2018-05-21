namespace PoESkillTree.Computation.Common.Builders.Forms
{
    /// <summary>
    /// Factory interface for forms.
    /// </summary>
    public interface IFormBuilders
    {
        /// <summary>
        /// Gets a form for setting the initial base value. It can only be set once. The base value defaults to 0 if not
        /// explicitly set.
        /// </summary>
        IFormBuilder BaseSet { get; }


        /// <summary>
        /// Gets a form for adding values to the base value.
        /// </summary>
        IFormBuilder BaseAdd { get; }

        /// <summary>
        /// Gets a form for subtracting values from the base value.
        /// </summary>
        IFormBuilder BaseSubtract { get; }


        /// <summary>
        /// Gets a form for increasing the base value by a percentage with additive accumulation.
        /// </summary>
        IFormBuilder PercentIncrease { get; }

        /// <summary>
        /// Gets a form for reducing the base value by a percentage with additive accumulation.
        /// </summary>
        IFormBuilder PercentReduce { get; }


        /// <summary>
        /// Gets a form for increasing the base value by a percentage with multiplicative accumulation.
        /// </summary>
        IFormBuilder PercentMore { get; }

        /// <summary>
        /// Gets a form for reducing the base value by a percentage with multiplicative accumulation.
        /// </summary>
        IFormBuilder PercentLess { get; }


        /// <summary>
        /// Gets a form for overriding the value (and discarding all other values). This applies after conversion.
        /// This form disregards the maximum and minimum values of stats.
        /// </summary>
        IFormBuilder TotalOverride { get; }

        /// <summary>
        /// Gets a form for overriding the base value (and discarding all BaseSet, BaseAdd, BaseSubtract, MinBaseAdd
        /// and MaxBaseAdd values). This form only makes sense with a value of 0 in most/all cases because percent forms
        /// still apply.
        /// </summary>
        IFormBuilder BaseOverride { get; }
    }
}