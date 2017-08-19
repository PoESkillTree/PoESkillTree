using System;
using System.Collections.Generic;
using PoESkillTree.Common;

namespace PoESkillTree.Computation.Parsing
{
    public class CompositeParser<TInnerResult, TResult> : IParser<TResult>
    {
        private readonly IFactory<IParsingSession<TInnerResult>> _sessionFactory;
        private readonly Func<IReadOnlyList<TInnerResult>, TResult> _resultAggregator;

        public CompositeParser(IFactory<IParsingSession<TInnerResult>> sessionFactory,
            Func<IReadOnlyList<TInnerResult>, TResult> resultAggregator)
        {
            _sessionFactory = sessionFactory;
            _resultAggregator = resultAggregator;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            var session = _sessionFactory.Create();
            remaining = stat;
            var results = new List<TInnerResult>();
            while (!session.Completed)
            {
                var parser = session.CurrentParser;
                if (parser.TryParse(remaining, out remaining, out var singleResult))
                {
                    results.Add(singleResult);
                    session.ParseSuccessful();
                }
                else
                {
                    session.ParseFailed();
                }
            }
            result = _resultAggregator(results);
            return session.Successful;
        }
    }
}