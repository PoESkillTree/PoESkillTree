using System;
using System.Collections.Concurrent;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public interface ITwoDArray<out T>
    {
        T this[int a, int b] { get; }
    }

    public class TwoDArray<T> : ITwoDArray<T>
    {
        private readonly T[,] _array;

        public T this[int a, int b]
        {
            get { return _array[a, b]; }
        }

        public TwoDArray(T[,] array)
        {
            _array = array;
        }
    }

    public class FunctionalTwoDArray<T> : ITwoDArray<T>
    {
        private readonly Func<int, int, T> _f;

        public T this[int a, int b]
        {
            get { return _f(a, b); }
        }

        public FunctionalTwoDArray(Func<int, int, T> f)
        {
            _f = f;
        }
    }

    /// <summary>
    /// Type of nodes used for the Priority Queue.
    /// The values represent the DistancesIndex of the two GraphNodes this edge connects.
    /// </summary>
    public class DirectedGraphEdge : IWithPriority
    {
        public readonly int Inside, Outside;

        public DirectedGraphEdge(int inside, int outside, uint priority)
        {
            Inside = inside;
            Outside = outside;
            Priority = priority;
        }

        public uint Priority { get; private set; }
    }

    public interface IWithPriority
    {
        uint Priority { get; }
    }
    
    // todo Redo Documentation
    /// <summary>
    /// Priority Queue based on a linked list and a lookup table for priorities.
    /// </summary>
    /// <remarks>
    /// Has O(1) Enqueue and Dequeue with respect to the number of nodes enqueued.
    /// 
    /// For enqueuing n nodes the runtime is linear in n and at most quadratic in the
    /// difference between the minimum and maximum priority of those nodes.
    /// 
    /// If the priorites are not discrete (no integer values) or have a range significantly bigger than
    /// the number of nodes being in the queue at one point, use HeapPriorityQueue.
    /// 
    /// No actual traversing through the linked list happens.
    /// </remarks>
    /// <typeparam name="T">Type of the stored objects</typeparam>
    public class LinkedListPriorityQueue<T> : IDisposable
        where T: IWithPriority
    {
        private struct DataArrays
        {
            public readonly T[] Nodes;

            public readonly int[] Links;

            public DataArrays(T[] nodes, int[] links)
            {
                Nodes = nodes;
                Links = links;
            }
        }

        private static readonly ConcurrentBag<DataArrays> DataStack = new ConcurrentBag<DataArrays>();

        private uint _top = uint.MaxValue;
        
        private int _count;

        public bool IsEmpty
        {
            get { return _count == 0; }
        }

        private readonly int[] _firstElements;

        private readonly int[] _lastElements;

        private readonly T[] _nodes;

        private readonly int[] _links;

        private int _nextId = 1;

        /// <summary>
        /// Creates a queue with the given maximum priority of stored nodes.
        /// </summary>
        /// <param name="maxPriority">The maximum priority of nodes that will be stored.
        /// If a node with a higher priority is to be stored, an exception will be thrown.
        /// The value has no impact on runtime, only on used memory.</param>
        /// <param name="size"></param>
        public LinkedListPriorityQueue(int maxPriority, int size)
        {
            _firstElements = new int[maxPriority + 1];
            _lastElements = new int[maxPriority + 1];
            DataArrays current;
            if (!DataStack.TryTake(out current))
            {
                _nodes = new T[size + 1];
                _links = new int[size + 1];
            }
            else if (current.Links.Length < size + 1)
            {
                var length = Math.Max(size + 1, current.Links.Length*2);
                _nodes = new T[length];
                _links = new int[length];
            }
            else
            {
                _links = current.Links;
                _nodes = current.Nodes;
            }
        }

        /// <summary>
        /// Enqueue the given node. Always O(1) with respect to the number of nodes stored
        /// but gets slower when the difference between the given and the next lower priority
        /// already stored gets larger.
        /// 
        /// Older nodes with the same priority are stored in front.
        /// </summary>
        /// <param name="node">The node to be stored.</param>
        public void Enqueue(T node)
        {
            var priority = node.Priority;
            var id = _nextId++;
            _nodes[id] = node;
            _links[id] = 0;

            _count++;
            var old = _lastElements[priority];
            if (old == 0)
            {
                _firstElements[priority] = id;
                if (priority < _top)
                {
                    _top = priority;
                }
            }
            else
            {
                _links[old] = id;
            }
            _lastElements[priority] = id;
        }

        /// <summary>
        /// Returns the node with the lowest priority and removes
        /// it from the queue. O(1)
        /// </summary>
        public T Dequeue()
        {
            _count--;
            var id = _firstElements[_top];
            var node = _nodes[id];
            var next = _links[id];
            if (next == 0)
            {
                _lastElements[_top] = 0;
                if (_count == 0)
                {
                    _top = uint.MaxValue;
                }
                else
                {
                    while (_lastElements[++_top] == 0)
                    {
                    }
                }
            }
            else
            {
                _firstElements[_top] = next;
            }
            _links[id] = 0;
            return node;
        }

        public void Dispose()
        {
            DataStack.Add(new DataArrays(_nodes, _links));
        }
    }
}