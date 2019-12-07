using System;
using System.Collections.Generic;

namespace MB.Algodat
{
    /// <summary>
    /// Range tree interface.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public interface IRangeTree<in TKey, T>
        where TKey : IComparable<TKey>
        where T : IRangeProvider<TKey>
    {
        List<T> Query(TKey value);
    }

    /// <summary>
    /// The standard range tree implementation. Keeps a root node and
    /// forwards all queries to it.
    /// Whenenver new items are added or items are removed, the tree 
    /// goes "out of sync" and is rebuild when it's queried next.
    /// </summary>
    /// <typeparam name="TKey">The type of the range.</typeparam>
    /// <typeparam name="T">The type of the data items.</typeparam>
    public class RangeTree<TKey, T> : IRangeTree<TKey, T>
        where TKey : IComparable<TKey>
        where T : IRangeProvider<TKey>
    {
        private readonly RangeTreeNode<TKey, T> _root;

        /// <summary>
        /// Initializes a tree with a list of items to be added.
        /// </summary>
        public RangeTree(IEnumerable<T> items, IComparer<T> rangeComparer)
        {
            _root = new RangeTreeNode<TKey, T>(items, rangeComparer);
        }

        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(TKey value)
        {
            return _root.Query(value);
        }
    }
}
