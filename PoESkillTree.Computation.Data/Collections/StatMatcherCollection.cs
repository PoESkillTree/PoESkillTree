using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatMatcherCollection : StatMatcherCollection<IStatProvider>
    {
        public StatMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }
    }


    public class StatMatcherCollection<T> : MatcherCollection where T : class, IStatProvider
    {
        public StatMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, params T[] stats)
        {
            Add(regex, (IEnumerable<T>) stats);
        }

        public void Add([RegexPattern] string regex, IEnumerable<T> stats)
        {
            var builder = MatchBuilder
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, T stat, string substitution = "")
        {
            var builder = MatchBuilder
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add([RegexPattern] string regex, T stat, IConditionProvider condition)
        {
            var builder = MatchBuilder
                .WithStat(stat)
                .WithCondition(condition);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, T stat, ValueFunc converter)
        {
            var builder = MatchBuilder
                .WithStat(stat)
                .WithValueConverter(converter);
            Add(regex, builder);
        }
    }
}