RangeTree
=========

## A generic and asynchronous interval tree

A generic implementation of a centered interval tree in C#. Also comes with an asynchronous version which rebuilds the tree using the Task Parallel Library (TPL).

From [Wikipedia](http://en.wikipedia.org/wiki/Interval_tree):
> In computer science, an interval tree is an ordered tree data structure to hold intervals. Specifically, it allows one to efficiently find all intervals that overlap with any given interval or point. It is often used for windowing queries, for instance, to find all roads on a computerized map inside a rectangular viewport, or to find all visible elements inside a three-dimensional scene.

Based on the Java implementation found here: http://www.thekevindolan.com/2010/02/interval-tree/index.html

Queries require O(log n + m) time, with n being the total number of intervals and m being the number of reported results. Construction requires O(n log n) time, and storage requires O(n) space.

### Requirements ###

.NET 4, Visual Studio 2010.

### Simple Interface ###

    public interface IRangeTree<TKey, T>
        where TKey : IComparable<TKey>
        where T : IRangeProvider<TKey>
    {
        IEnumerable<T> Items { get; }
        int Count { get; }

        List<T> Query(TKey value);
        List<T> Query(Range<TKey> range);

        void Rebuild();
        void Add(T item);
        void Add(IEnumerable<T> items);
        void Remove(T item);
        void Remove(IEnumerable<T> items);
        void Clear();
    }
    
### Example ###

    var tree = new RangeTree<int, RangeItem>(new RangeItemComparer());

    tree.Add(new RangeItem(0, 10, "1"));
    tree.Add(new RangeItem(20, 30, "2"));
    tree.Add(new RangeItem(15, 17, "3"));
    tree.Add(new RangeItem(25, 35, "4"));

    var results1 = tree.Query(5);                     // 1 item: [0 - 10]
    var results2 = tree.Query(10);                    // 1 item: [0 - 10]
    var results3 = tree.Query(29);                    // 2 items: [20 - 30], [25 - 35]
    var results4 = tree.Query(new Range<int>(5, 15)); // 2 items: [0 - 10], [15 - 17]
    
The solution file contains a few examples and also a comparision of the default and async versions.
    
### Implementation Details ###

In the standard implementation, whenever you add or remove items from the tree, the tree goes "out of sync". Whenever it is queried next, the tree structure is then automatically rebuild (you can control this behaviour using the `AutoRebuild` flag). You may also call `Rebuild()` manually.
The creation of the tree requires O(n log n) time. Therefore, the standard implementation is best suited for trees that do not change often or small trees, where the creation time is negligible.

For other cases, use the asynchronous version: `RangeTreeAsync`. Usage is exactly the same but it uses asynchronous tasks to build the tree. Only one task is started simultaneously, if another additional rebuild is needed, another async task is started at the end.
Whenever a query is started on the tree while its being rebuilt, the tree first queries the the outdated tree and then additionally checks all items which were added since the last rebuild (this is a sequential operation and takes O(n) time). Similary, results are removed if the item has been removed since the last rebuild.
This works especially well when only small numbers of items are added. Do not use it if you add millions of items every second.
