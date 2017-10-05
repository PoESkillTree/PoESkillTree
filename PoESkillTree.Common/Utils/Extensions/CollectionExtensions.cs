using System;
using System.Collections.Generic;

namespace PoESkillTree.Common.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, 
            TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dict[key] = value;
            }
            return value;
        }
    }
}