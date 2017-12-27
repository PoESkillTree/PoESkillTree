using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    /// <summary>
    /// Interface that contains all data objects that are required for parsing stat lines.
    /// </summary>
    /// <typeparam name="TStep">The type of steps that represent states in <see cref="Stepper"/> and can be
    /// used to select stat matchers for steps using <see cref="SelectStatMatcher"/>.</typeparam>
    /// <remarks>
    /// The combination of <see cref="Stepper"/> and <see cref="SelectStatMatcher"/> must make sure that
    /// <see cref="IStepper{T}.IsSuccess"/> only returns true if one matched <see cref="MatcherData.Modifier"/>
    /// has stats in each entry, one has forms in each entry and one has values in each entry. The resulting
    /// <see cref="Modifier"/>s must have a stat, form and value (and condition, but that is taken care of by
    /// adding a default condition when building the merged <see cref="ModifierBuilding.IIntermediateModifier"/>).
    /// </remarks>
    public interface IParsingData<TStep>
    {
        IReadOnlyList<IReferencedMatchers> ReferencedMatchers { get; }

        IReadOnlyList<IStatMatchers> StatMatchers { get; }

        IReadOnlyList<StatReplacerData> StatReplacers { get; }

        IStepper<TStep> Stepper { get; }

        /// <returns>The <see cref="IStatMatchers"/> instance from <see cref="StatMatchers"/> that belongs to the 
        /// given step.</returns>
        IStatMatchers SelectStatMatcher(TStep step);
    }
}