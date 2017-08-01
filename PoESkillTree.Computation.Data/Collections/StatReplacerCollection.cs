using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PoESkillTree.Computation.Data.Collections
{
    public class StatReplacerCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add([RegexPattern] string originalStat, params string[] replacements)
        {
            throw new NotImplementedException();
        }
    }
}