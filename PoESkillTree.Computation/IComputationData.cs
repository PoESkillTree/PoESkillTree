using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    // Data structures and data in separate projects to allow hiding implementation with internal

    // Interfaces have "Provider" in name as they may be dependent on the values and/or groups of the
    // actually matched stat line and to not lead to confusion with existing classes.

    public interface IComputationData
    {
        IReadOnlyList<string> GivenStats { get; }

        FormMatcherCollection FormMatchers { get; }

        FormMatcherCollection FormAndStatMatchers { get; }

        StatMatcherCollection RegenTypeMatchers { get; }

        StatMatcherCollection StatMatchers { get; }

        BuffMatcherCollection BuffMatchers { get; }

        ConditionMatcherCollection ConditionMatchers { get; }

        MultiplierMatcherCollection MultiplierMatchers { get; }

        SpecialMatcherCollection SpecialMatchers { get; }

        StatMatcherCollection PropertyMatchers { get; }
    }
}