using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatMatcherCollection : StatMatcherCollection<IStatBuilder>
    {
        public StatMatcherCollection(IModifierBuilder modifierBuilder,
            IValueBuilders valueFactory) : base(modifierBuilder, valueFactory)
        {
        }
    }


    public class StatMatcherCollection<T> : MatcherCollection where T : class, IStatBuilder
    {
        private readonly IValueBuilders _valueFactory;

        public StatMatcherCollection(IModifierBuilder modifierBuilder,
            IValueBuilders valueFactory) : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, params T[] stats)
        {
            Add(regex, (IEnumerable<T>) stats);
        }

        public void Add([RegexPattern] string regex, IEnumerable<T> stats)
        {
            var builder = ModifierBuilder
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, T stat, string substitution = "")
        {
            var builder = ModifierBuilder
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add([RegexPattern] string regex, T stat, IConditionBuilder condition)
        {
            var builder = ModifierBuilder
                .WithStat(stat)
                .WithCondition(condition);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, T stat, 
            Func<ValueBuilder, ValueBuilder> converter)
        {
            var builder = ModifierBuilder
                .WithStat(stat)
                .WithValueConverter(_valueFactory.WrapValueConverter(converter));
            Add(regex, builder);
        }
    }
}