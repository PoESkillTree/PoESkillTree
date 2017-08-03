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
    // TODO rename collision between generic and non-generic MatcherCollection

    public class MatcherCollection : IEnumerable<MatcherData>
    {
        public IEnumerator<MatcherData> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class FormMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IFormProvider form, int? value = null)
        {
            throw new NotImplementedException();
        }
    }


    public class FormAndStatMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, double? value = null,
            IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, params IStatProvider[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IEnumerable<IStatProvider> stats)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, string substitution)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, ValueFunc converter)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, 
            (IFormProvider forFirstValue, IFormProvider forSecondValue) forms, IStatProvider stat)
        {
            throw new NotImplementedException();
        }
    }


    public class StatMatcherCollection<T> : MatcherCollection where T : class, IStatProvider
    {
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
    }


    public class PropertyMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IStatProvider stat = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IStatProvider stat, ValueFunc converter)
        {
            throw new NotImplementedException();
        }
    }


    public class MatcherCollection<T> : IEnumerable<ReferencedMatcherData<T>>
    {
        public void Add([RegexPattern] string regex, T element)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ReferencedMatcherData<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class ConditionMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IConditionProvider condition)
        {
            throw new NotImplementedException();
        }
    }


    public class ValueConversionMatcherCollection : MatcherCollection
    {
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