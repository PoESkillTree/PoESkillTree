using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FlagStatCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IFlagStatProvider stat, params string[] stats)
        {
            throw new NotImplementedException();
        }
    }
}