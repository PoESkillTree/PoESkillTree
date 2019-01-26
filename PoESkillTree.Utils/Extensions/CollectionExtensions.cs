using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Utils.Extensions
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

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair,
            out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static void Deconstruct<TKey, TValue>(this IGrouping<TKey, TValue> pair,
            out TKey key, out IEnumerable<TValue> value)
        {
            key = pair.Key;
            value = pair;
        }

        /// <summary>
        /// Applies <paramref name="action"/> to <c>dict[key]</c> if <c>dict.ContainsKey(key)</c>.
        /// </summary>
        public static void ApplyIfPresent<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey key, Action<TValue> action)
        {
            if (dict.TryGetValue(key, out var value))
            {
                action(value);
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict,
            TKey key, TValue defaultValue = default)
            => dict.TryGetValue(key, out var value) ? value : defaultValue;

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict,
            TKey key, Func<TValue> defaultValueProvider)
            => dict.TryGetValue(key, out var value) ? value : defaultValueProvider();
    }
}