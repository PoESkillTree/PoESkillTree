using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatManipulatorMatcherCollection : MatcherCollection
    {
        public StatManipulatorMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        public void Add([RegexPattern] string regex,
            Func<IStatBuilder, IStatBuilder> manipulateStat,
            string substitution = "")
        {
            var builder = ModifierBuilder
                .WithStatConverter(manipulateStat);
            Add(regex, builder, substitution);
        }

        public void Add<T>([RegexPattern] string regex, 
            Func<T, IStatBuilder> manipulateStat, 
            string substitution = "") where T: IStatBuilder
        {
            // needs to verify that the matched mod line's stat is of type T
            Add(regex,
                s => (s is T t)
                    ? manipulateStat(t)
                    : throw new ParseException(
                        $"Can only manipulate stats of type {typeof(T)}, was {s?.GetType()} (regex={regex}, stat={s})"),
                substitution);
        }
    }
}