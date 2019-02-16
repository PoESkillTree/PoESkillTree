using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
            => !enumerable.Any();

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
            => enumerable.SelectMany(ts => ts);

        public static bool All(this IEnumerable<bool> enumerable)
            => enumerable.All(b => b);

        public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, params T[] elements) => 
            @this.Concat(elements);

        public static IEnumerable<T> Except<T>(this IEnumerable<T> @this, params T[] elements) =>
            @this.Except((IEnumerable<T>) elements);

        public static bool ContainsAll<T>(this IEnumerable<T> @this, IReadOnlyCollection<T> elements) =>
            @this.Intersect(elements).Count() == elements.Count;

        public static bool ContainsAny<T>(this IEnumerable<T> @this, params T[] elements) =>
            @this.ContainsAny(elements.AsEnumerable());

        public static bool ContainsAny<T>(this IEnumerable<T> @this, IEnumerable<T> elements) =>
            @this.Intersect(elements).Any();

        public static bool ContainsNone<T>(this IEnumerable<T> @this, IEnumerable<T> elements) =>
            !@this.ContainsAny(elements);

        public static bool SequenceEqual<T>(this IReadOnlyList<T> @this, IReadOnlyList<T> other)
        {
            if (@this.Count != other.Count)
                return false;

            for (var i = 0; i < @this.Count; i++)
            {
                if (!@this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for <paramref name="values"/> that can be used in conjunction with
        /// <see cref="SequenceEqual{TSource}(IReadOnlyList{TSource},IReadOnlyList{TSource})"/>.
        /// </summary>
        public static int SequenceHash<T>(this IReadOnlyList<T> values)
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