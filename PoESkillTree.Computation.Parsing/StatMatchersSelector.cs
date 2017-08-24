using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing
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
            return (
                from c in _candidates
                let name = c.GetType().Name
                where name.StartsWith(parsingStep.ToString())
                orderby name.Length
                select c
            ).First();
        }
    }
}