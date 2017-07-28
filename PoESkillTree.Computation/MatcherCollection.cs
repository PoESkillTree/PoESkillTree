using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using PoESkillTree.Computation.Providers;

namespace PoESkillTree.Computation
{
    public class MatcherCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
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

        public void Add([RegexPattern] string regex, T stat = null, string substitution = "")
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

        public void Add([RegexPattern] string regex, T stat, IMatchConditionProvider matchesIf)
        {
            throw new NotImplementedException();
        }
    }


    public class StatMatcherCollection : StatMatcherCollection<IStatProvider>
    {
    }


    public class MatcherCollection<T> : MatcherCollection
    {
        public void Add([RegexPattern] string regex, T element)
        {
            throw new NotImplementedException();
        }
    }


    public class DamageTypeMatcherCollection : MatcherCollection<IDamageTypeProvider>
    {
    }


    public class ChargeTypeMatcherCollection : MatcherCollection<IChargeTypeProvider>
    {
    }


    public class AilmentMatcherCollection : MatcherCollection<IAilmentProvider>
    {
    }


    public class FlagMatcherCollection : MatcherCollection<IFlagStatProvider>
    {
    }


    public class KeywordMatcherCollection : MatcherCollection<IKeywordProvider>
    {
    }


    public class ConditionMatcherCollection : MatcherCollection<IConditionProvider>
    {
    }


    public class ValueConversionMatcherCollection : MatcherCollection<ValueFunc>
    {
        public void Add([RegexPattern] string regex, ValueProvider multiplier)
        {
            Add(regex, v => v * multiplier);
        }
    }


    public class ActionMatcherCollection : MatcherCollection<IActionProvider>
    {
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
        public void Add([RegexPattern] string regex, IGemModifierProvider gemModifier)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, 
            ValueProvider value, ValueFunc converter = null, IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            ValueProvider value, IConditionProvider condition)
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
    }
}