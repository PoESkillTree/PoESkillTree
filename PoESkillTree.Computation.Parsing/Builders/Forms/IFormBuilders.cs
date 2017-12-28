namespace PoESkillTree.Computation.Parsing.Builders.Forms
{
    /// <summary>
    /// Factory interface for forms.
    /// </summary>
    public interface IFormBuilders
    {
        /// <summary>
        /// Gets a form for setting the intial base value. It can only be set once. The base value defaults to 0 if not
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
        /// Gets a form for adding values to the damage stat's minimum base value.
        /// </summary>
        IFormBuilder MinBaseAdd { get; }

        /// <summary>
        /// Gets a form for adding values to the damage stat's maximum base value.
        /// </summary>
        IFormBuilder MaxBaseAdd { get; }


        /// <summary>
        /// Gets a form for overriding the value (and discarding all other values). For damage stats, this applies
        /// after conversion. This form disregards the maximum and minimum values of stats.
        /// </summary>
        IFormBuilder TotalOverride { get; }
    }
}