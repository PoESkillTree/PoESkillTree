using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Collections
{
    public class EffectStatCollection : IEnumerable<object>
    {
        public IEnumerator<object> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IEffectProvider effect, params string[] stats)
        {
            throw new NotImplementedException();
        }

        public void Add(IEffectProvider effect, params IFlagStatProvider[] stats)
        {
            throw new NotImplementedException();
        }
    }
}