using System.Collections;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Data.Collections
{
    public abstract class MatcherCollection : IEnumerable<MatcherData>
    {
        protected IMatchBuilder MatchBuilder { get; }

        private readonly List<MatcherData> _matchers = new List<MatcherData>();

        protected MatcherCollection(IMatchBuilder matchBuilder)
        {
            MatchBuilder = matchBuilder;
        }

        protected void Add(string regex, IMatchBuilder matchBuilder)
        {
            _matchers.Add(new MatcherData(regex, matchBuilder));
        }

        protected void Add(string regex, IMatchBuilder matchBuilder, string matchSubstitution)
        {
            _matchers.Add(new MatcherData(regex, matchBuilder, matchSubstitution));
        }

        public IEnumerator<MatcherData> GetEnumerator()
        {
            return _matchers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}