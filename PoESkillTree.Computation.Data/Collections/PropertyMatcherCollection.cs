using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class PropertyMatcherCollection : MatcherCollection
    {
        public PropertyMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex)
        {
            Add(regex, MatchBuilder);
        }

        public void Add([RegexPattern] string regex, IStatProvider stat)
        {
            Add(regex, MatchBuilder.WithStat(stat));
        }

        public void Add([RegexPattern] string regex, IStatProvider stat, ValueFunc converter)
        {
            var builder = MatchBuilder
                .WithStat(stat)
                .WithValueConverter(converter);
            Add(regex, builder);
        }
    }
}