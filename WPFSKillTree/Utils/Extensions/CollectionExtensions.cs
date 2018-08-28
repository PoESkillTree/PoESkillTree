using System.Collections.Generic;
using MoreLinq;

namespace POESKillTree.Utils.Extensions
{
    /// <summary>
    /// Extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            toAdd.ForEach(collection.Add);
        }
    }
}