using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="Common.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of a stat converter, that allows collection 
    /// initialization syntax for adding entries.
    /// </summary>
    public class StatManipulatorMatcherCollection : MatcherCollection
    {
        public StatManipulatorMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        /// <summary>
        /// Adds a matcher applying a stat converter.
        /// </summary>
        public void Add(
            [RegexPattern] string regex,
            StatConverter manipulateStat,
            string substitution = "")
        {
            var builder = ModifierBuilder
                .WithStatConverter(manipulateStat);
            Add(regex, builder, substitution);
        }

        /// <summary>
        /// Adds a matcher applying a stat converter to stats of type <typeparamref name="T"/>. If the converter
        /// is applied to a stat that is not of type <typeparamref name="T"/>, a <see cref="ParseException"/> is thrown
        /// when trying to convert it.
        /// </summary>
        public void Add<T>(
            [RegexPattern] string regex,
            Func<T, IStatBuilder> manipulateStat,
            string substitution = "") where T : IStatBuilder
        {
            IStatBuilder ConvertStat(IStatBuilder stat)
            {
                if (stat is T t)
                {
                    return manipulateStat(t);
                }

                throw new ParseException(
                    $"Can only manipulate stats of type {typeof(T)}, was {stat?.GetType()} (regex={regex}, stat={stat})");
            }

            Add(regex, ConvertStat, substitution);
        }
    }
}