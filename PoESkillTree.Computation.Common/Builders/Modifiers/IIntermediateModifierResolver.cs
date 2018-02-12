using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <summary>
    /// Resolves parts of <see cref="IIntermediateModifier"/> via the <see cref="IResolvable{T}"/> interface.
    /// </summary>
    public interface IIntermediateModifierResolver
    {
        /// <returns>A new intermediate modifier identical to <paramref name="unresolved"/> except with all builders of 
        /// all entries resolved. Because the converters cannot be called yet, the new converters are equivalent
        /// to the old ones except calling <see cref="IResolvable{T}.Resolve"/> on non-null results.</returns>
        IIntermediateModifier Resolve(IIntermediateModifier unresolved, ResolveContext context);

        /// <returns>The resolved stat of the only entry of <paramref name="unresolved"/>. 
        /// <paramref name="unresolved"/>'s <see cref="IIntermediateModifier.StatConverter"/> is applied to it
        /// and the entries conditions is added with <see cref="IStatBuilder.WithCondition"/>, if existing, before
        /// resolving. <paramref name="unresolved"/> must have a single entry with no value, no form and a stat.
        /// </returns>
        IStatBuilder ResolveToReferencedBuilder(IIntermediateModifier unresolved, ResolveContext context);
    }
}