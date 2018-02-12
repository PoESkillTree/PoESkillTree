using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="Common.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of one or more stats or a stat and a condition, 
    /// that allows collection initialization syntax for adding entries.
    /// <para>The stats must be of type <typeparamref name="T"/>.</para>
    /// </summary>
    /// <typeparam name="T">The type of stats passed to methods of this class.</typeparam>
    public class StatMatcherCollection<T> : MatcherCollection where T : class, IStatBuilder
    {
        public StatMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
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
    }
}