using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Steps
{
    /// <summary>
    /// Maps <see cref="ParsingStep"/> to <see cref="IStatMatchers"/> instances using the instances' type names.
    /// If multiple start with the same step, the longest name wins.
    /// </summary>
    public class StatMatchersSelector
    {
        private readonly IReadOnlyList<IStatMatchers> _candidates;

        public StatMatchersSelector(IReadOnlyList<IStatMatchers> candidates)
        {
            _candidates = candidates;
        }

        /// <summary>
        /// Selects the <see cref="IStatMatchers"/> instance best matching the given <see cref="ParsingStep"/>.
        /// </summary>
        public IStatMatchers Get(ParsingStep parsingStep)
        {
            var asString = parsingStep.ToString();
            return (
                from c in _candidates
                let name = c.GetType().Name
                where name.StartsWith(asString)
                orderby name.Length
                select c
            ).First();
        }
    }
}