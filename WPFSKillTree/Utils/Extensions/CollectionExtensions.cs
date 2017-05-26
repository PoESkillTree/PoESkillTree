using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace POESKillTree.Utils.Extensions
{
    /// <summary>
    /// Extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            return enumerable.SelectMany(e => e);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            toAdd.ForEach(collection.Add);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key,
            TValue defaultValue = default(TValue))
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key,
            Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValueProvider();
        }

        public static bool All(this IEnumerable<bool> enumerable)
        {
            return enumerable.All(b => b);
        }
    }
}