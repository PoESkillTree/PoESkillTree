using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Steps
{
    /// <summary>
    /// Maps <see cref="ParsingStep"/> to <see cref="IStatMatchers"/> instances using the instances' type names.
    /// If multiple start with the same step, the longest name wins.
    /// </summary>
    public class StatMatchersSelector
    {
        private readonly Lazy<IReadOnlyList<IStatMatchers>> _orderedCandidates;

        public StatMatchersSelector(IReadOnlyList<IStatMatchers> candidates)
        {
            _orderedCandidates = new Lazy<IReadOnlyList<IStatMatchers>>(
                () => candidates.OrderBy(c => c.GetType().Name.Length).ToList());
        }

        /// <summary>
        /// Selects the <see cref="IStatMatchers"/> instance best matching the given <see cref="ParsingStep"/>.
        /// </summary>
        public IStatMatchers Get(ParsingStep parsingStep)
        {
            var asString = parsingStep.GetName();
            return _orderedCandidates.Value
                .First(c => c.GetType().Name.StartsWith(asString, StringComparison.Ordinal));
        }
    }
}