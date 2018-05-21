using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Common.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            return enumerable.SelectMany(ts => ts);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, T element) => 
            @this.Union(new[] { element });

        /// <summary>
        /// Returns a hash code for <paramref name="values"/> that can be used in conjunction with
        /// <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource},IEnumerable{TSource})"/>.
        /// </summary>
        public static int SequenceHash<T>(this IEnumerable<T> values)
        {
            unchecked
            {
                int hash = 19;
                foreach (var value in values)
                {
                    hash = hash * 31 + (value?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}