using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IParsingData<TStep>
    {
        IReadOnlyList<IReferencedMatchers> ReferencedMatchers { get; }

        IReadOnlyList<IStatMatchers> StatMatchers { get; }

        IReadOnlyList<StatReplacerData> StatReplacers { get; }

        IStepper<TStep> Stepper { get; }

        IStatMatchers SelectStatMatcher(TStep step);
    }
}