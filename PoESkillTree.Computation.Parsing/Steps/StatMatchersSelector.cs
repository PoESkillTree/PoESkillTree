using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing.Steps
{
    public class StatMatchersSelector
    {
        private readonly IReadOnlyList<IStatMatchers> _candidates;

        public StatMatchersSelector(params IStatMatchers[] candidates)
            : this((IReadOnlyList<IStatMatchers>) candidates)
        {
        }

        public StatMatchersSelector(IReadOnlyList<IStatMatchers> candidates)
        {
            _candidates = candidates;
        }

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