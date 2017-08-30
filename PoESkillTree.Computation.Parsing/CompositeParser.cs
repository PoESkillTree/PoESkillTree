using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing
{
    public class CompositeParser<TInnerResult> : IParser<IReadOnlyList<TInnerResult>>
    {
        private readonly IStep<IParser<TInnerResult>, bool> _initialStep;

        public CompositeParser(IStep<IParser<TInnerResult>, bool> initialStep)
        {
            _initialStep = initialStep;
        }

        public bool TryParse(string stat, out string remaining, out IReadOnlyList<TInnerResult> result)
        {
            var step = _initialStep;
            remaining = stat;
            var results = new List<TInnerResult>();
            while (!step.Completed)
            {
                var parser = step.Current;
                var parserReturn = parser.TryParse(remaining, out remaining, out var singleResult);
                if (parserReturn)
                {
                    results.Add(singleResult);
                }
                step = step.Next(parserReturn);
            }
            result = results;
            return step.Successful;
        }
    }
}