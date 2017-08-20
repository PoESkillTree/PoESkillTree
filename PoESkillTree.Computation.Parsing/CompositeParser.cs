using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing
{
    public class CompositeParser<TInnerResult, TResult> : IParser<TResult>
    {
        private readonly IStep<IParser<TInnerResult>, bool> _initialStep;
        private readonly Func<IReadOnlyList<TInnerResult>, TResult> _resultAggregator;

        public CompositeParser(IStep<IParser<TInnerResult>, bool> initialStep,
            Func<IReadOnlyList<TInnerResult>, TResult> resultAggregator)
        {
            _initialStep = initialStep;
            _resultAggregator = resultAggregator;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
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
            result = _resultAggregator(results);
            return step.Successful;
        }
    }
}