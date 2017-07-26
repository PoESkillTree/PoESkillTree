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


    public class DamageTypeMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IDamageTypeProvider type)
        {
            throw new NotImplementedException();
        }
    }


    public class ChargeTypeMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IChargeTypeProvider type)
        {
            throw new NotImplementedException();
        }
    }


    public class AilmentMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IAilmentProvider type)
        {
            throw new NotImplementedException();
        }
    }


    public class FlagMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IFlagStatProvider flagStat)
        {
            throw new NotImplementedException();
        }
    }


    public class KeywordMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IKeywordProvider keyword)
        {
            throw new NotImplementedException();
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
        public void Add([RegexPattern] string regex, ValueFunc conversionFunc)
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

        public void Add<T>([RegexPattern] string regex,
            Func<IStatProvider, T[]> manipulateStat,
            string substitution = "") where T: IStatProvider
        {
            throw new NotImplementedException();
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