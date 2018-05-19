using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Forms
{
    /// <summary>
    /// Represents a form of applying values, e.g. percent increase or base add.
    /// </summary>
    public interface IFormBuilder : IResolvable<IFormBuilder>
    {
        /// <summary>
        /// Builds this instance into a <see cref="Form"/> and a <see cref="ValueConverter"/> that should be applied
        /// to <see cref="Values.IValueBuilder"/>s before building them.
        /// </summary>
        (Form form, ValueConverter valueConverter) Build();
    }
}