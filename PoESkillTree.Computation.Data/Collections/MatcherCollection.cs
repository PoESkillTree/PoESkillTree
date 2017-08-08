using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

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


    public class StatMatcherCollection<T> : MatcherCollection where T : class, IStatProvider
    {
        public StatMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, params T[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IEnumerable<T> stats)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, T stat, string substitution = "")
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, T stat, IConditionProvider condition)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, T stat, ValueFunc converter)
        {
            throw new NotImplementedException();
        }
    }


    public class StatMatcherCollection : StatMatcherCollection<IStatProvider>
    {
        public StatMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }
    }


    public class PropertyMatcherCollection : MatcherCollection
    {
        public PropertyMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, IStatProvider stat = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IStatProvider stat, ValueFunc converter)
        {
            throw new NotImplementedException();
        }
    }


    public class ConditionMatcherCollection : MatcherCollection
    {
        public ConditionMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, IConditionProvider condition)
        {
            throw new NotImplementedException();
        }
    }


    public class ValueConversionMatcherCollection : MatcherCollection
    {
        public ValueConversionMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, ValueFunc func)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, ValueProvider multiplier)
        {
            Add(regex, v => v * multiplier);
        }
    }


    public class StatManipulatorMatcherCollection : MatcherCollection
    {
        public StatManipulatorMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex,
            Func<IStatProvider, IStatProvider> manipulateStat,
            string substitution = "")
        {
            throw new NotImplementedException();
        }

        public void Add<T>([RegexPattern] string regex, 
            Func<T, IStatProvider> manipulateStat, 
            string substitution = "") where T: IStatProvider
        {
            // needs to verify that the matched mod line's stat is of type T
            Add(regex, 
                s => (s is T t) ? manipulateStat(t) : throw new NotImplementedException(), 
                substitution);
        }
    }


    public class SpecialMatcherCollection : MatcherCollection
    {
        public SpecialMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, 
            ValueProvider value, ValueFunc converter = null, IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            double value, ValueFunc converter = null, IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            ValueProvider value, IConditionProvider condition)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            double value, IConditionProvider condition)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex,
            params (IFormProvider form, IStatProvider stat, ValueProvider value,
                IConditionProvider condition)[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex,
            params (IFormProvider form, IStatProvider stat, double value,
                IConditionProvider condition)[] stats)
        {
            throw new NotImplementedException();
        }
    }
}