using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public abstract class MatcherCollection : IEnumerable<MatcherData>
    {
        protected IModifierBuilder ModifierBuilder { get; }

        private readonly List<MatcherData> _matchers = new List<MatcherData>();

        protected MatcherCollection(IModifierBuilder modifierBuilder)
        {
            ModifierBuilder = modifierBuilder;
        }

        protected void Add(string regex, IModifierBuilder modifierBuilder)
        {
            _matchers.Add(new MatcherData(regex, modifierBuilder));
        }

        protected void Add(string regex, IModifierBuilder modifierBuilder, string matchSubstitution)
        {
            _matchers.Add(new MatcherData(regex, modifierBuilder, matchSubstitution));
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