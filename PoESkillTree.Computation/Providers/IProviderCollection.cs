using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers
{
    public interface IProviderCollection<out T> : IEnumerable<T>
    {
        ValueProvider Count(Func<T, IConditionProvider> predicate = null);

        IConditionProvider Any(Func<T, IConditionProvider> predicate = null);
    }
}