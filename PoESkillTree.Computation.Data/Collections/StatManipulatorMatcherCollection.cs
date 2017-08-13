using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatManipulatorMatcherCollection : MatcherCollection
    {
        public StatManipulatorMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex,
            Func<IStatProvider, IStatProvider> manipulateStat,
            string substitution = "")
        {
            var builder = MatchBuilder
                .WithStatConverter(manipulateStat);
            Add(regex, builder, substitution);
        }

        public void Add<T>([RegexPattern] string regex, 
            Func<T, IStatProvider> manipulateStat, 
            string substitution = "") where T: IStatProvider
        {
            // needs to verify that the matched mod line's stat is of type T
            Add(regex, 
                s => (s is T t) ? manipulateStat(t) : throw new NotSupportedException(), 
                substitution);
        }
    }
}