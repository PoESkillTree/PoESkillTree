using System;
using System.Collections.Generic;

namespace MB.Algodat
{
    /// <summary>
    /// A node of the range tree. Given a list of items, it builds
    /// its subtree. Also contains methods to query the subtree.
    /// Basically, all interval tree logic is here.
    /// </summary>
    public class RangeTreeNode<TKey, T>
        where TKey : IComparable<TKey>
        where T : IRangeProvider<TKey>
    {
        private readonly TKey _center;
        private readonly RangeTreeNode<TKey, T>? _leftNode;
        private readonly RangeTreeNode<TKey, T>? _rightNode;
        private readonly List<T>? _items;

        private static IComparer<T>? _rangeComparer;

        /// <summary>
        /// Initializes a node with a list of items, builds the sub tree.
        /// </summary>
        public RangeTreeNode(IEnumerable<T> items, IComparer<T>? rangeComparer = null)
        {
            if (rangeComparer != null)
                _rangeComparer = rangeComparer;

            // first, find the median
            var endPoints = new List<TKey>();
            foreach (var o in items)
            {
                var range = o.Range;
                endPoints.Add(range.From);
                endPoints.Add(range.To);
            }
            endPoints.Sort();

            // the median is used as center value
            _center = endPoints[endPoints.Count / 2];
            _items = new List<T>();

            var left = new List<T>();
            var right = new List<T>();

            // iterate over all items
            // if the range of an item is completely left of the center, add it to the left items
            // if it is on the right of the center, add it to the right items
            // otherwise (range overlaps the center), add the item to this node's items
            foreach (var o in items)
            {
                var range = o.Range;

                if (range.To.CompareTo(_center) < 0)
                    left.Add(o);
                else if (range.From.CompareTo(_center) > 0)
                    right.Add(o);
                else
                    _items.Add(o);
            }

            // sort the items, this way the query is faster later on
            if (_items.Count > 0)
                _items.Sort(_rangeComparer);
            else
                _items = null;

            // create left and right nodes, if there are any items
            if (left.Count > 0)
                _leftNode = new RangeTreeNode<TKey, T>(left);
            if (right.Count > 0)
                _rightNode = new RangeTreeNode<TKey, T>(right);
        }

        /// <summary>
        /// Performans a "stab" query with a single value.
        /// All items with overlapping ranges are returned.
        /// </summary>
        public List<T> Query(TKey value)
        {
            var results = new List<T>();

            // If the node has items, check their ranges.
            if (_items != null)
            {
                foreach (var o in _items)
                {
                    if (o.Range.From.CompareTo(value) > 0)
                        break;
                    else if (o.Range.Contains(value))
                        results.Add(o);
                }
            }

            // go to the left or go to the right of the tree, depending
            // where the query value lies compared to the center
            if (value.CompareTo(_center) < 0 && _leftNode != null)
                results.AddRange(_leftNode.Query(value));
            else if (value.CompareTo(_center) > 0 && _rightNode != null)
                results.AddRange(_rightNode.Query(value));

            return results;
        }
    }
}
