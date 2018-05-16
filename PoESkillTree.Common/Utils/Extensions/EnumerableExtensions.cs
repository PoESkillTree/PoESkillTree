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