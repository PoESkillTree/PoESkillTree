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
        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat = null)
        {
            throw new NotImplementedException();
        }
    }


    public class StatMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IStatProvider stat = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IStatProvider stat, IConverterProvider converter)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IStatProvider stat, IMatchConditionProvider matchesIf)
        {
            throw new NotImplementedException();
        }
    }


    public class BuffMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IBuffProvider buff)
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


    public class MultiplierMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IMultiplierProvider multiplier)
        {
            throw new NotImplementedException();
        }
    }


    public class SpecialMatcherCollection : MatcherCollection
    {
        public void Add([RegexPattern] string regex, IBuffProvider buff, IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IGemModifierProvider gemModifier)
        {
            throw new NotImplementedException();
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, IValueProvider value,
            IConditionProvider condition = null)
        {
            throw new NotImplementedException();
        }
    }
}