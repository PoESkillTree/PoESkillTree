using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ValueConversionMatcherCollection : MatcherCollection
    {
        public ValueConversionMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, ValueFunc func)
        {
            Add(regex, MatchBuilder.WithValueConverter(func));
        }

        public void Add([RegexPattern] string regex, ValueProvider multiplier)
        {
            Add(regex, v => v * multiplier);
        }
    }
}