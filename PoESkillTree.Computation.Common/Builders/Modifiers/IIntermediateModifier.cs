using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <summary>
    /// Collection of pre-stages of <see cref="Modifier"/>s. Each <see cref="IntermediateModifierEntry"/> can directly
    /// be build to a <see cref="Modifier"/>. However, this interface allows merging because it keeps related partial 
    /// modifiers together and doesn't apply stat/value converters.
    /// <para> E.g. there may be an intermediate modifier with two entries for two stats (from GeneralStatMatchers),
    /// one intermediate modifier with one entry for the form and value (from FormMatchers) and one with a
    /// value converter (from ValueConversionMatchers) and no entries. Without an intermediate interface,
    /// merging all of that into two <see cref="Modifier"/>s would not be possible. Also, later on, instances of the
    /// builder interfaces will need to build something to create the types of <see cref="Modifier"/>'s properties,
    /// which is not possible before resolving values and references.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The conversion functions are identity functions if they perform no conversion.
    /// </remarks>
    public interface IIntermediateModifier
    {
        IReadOnlyList<IntermediateModifierEntry> Entries { get; }

        Func<IStatBuilder, IStatBuilder> StatConverter { get; }

        Func<IValueBuilder, IValueBuilder> ValueConverter { get; }
    }
}