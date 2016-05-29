using System;
using System.Collections.Concurrent;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Type of nodes mainly used for the Priority Queue.
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

    /// <summary>
    /// Interface for nodes used with <see cref="LinkedListPriorityQueue{T}"/>.
    /// </summary>
    public interface IWithPriority
    {
        uint Priority { get; }
    }
    
    /// <summary>
    /// Priority Queue based on a linked list for each priority.
    /// </summary>
    /// <remarks>
    /// Has O(1) Enqueue and Dequeue with respect to the number of elements enqueued.
    /// 
    /// Dequeue has O(|max priority - min priority|) worst case runtime. It is only non-constant if the linked list for
    /// the last lowest priority is empty and the next lowest priority has to be found. That means, if it
    /// is used for sorting (no enqueue happens after the first dequeue), sorting n elements takes
    /// O(n + |max priority - min priority|) time.
    /// 
    /// If the priorites are not discrete (no integer values) or have a priority range significantly bigger than
    /// the number of nodes being in the queue at one point, use HeapPriorityQueue.
    /// 
    /// No actual traversing through the linked list happens.
    /// 
    /// As the queue represents entries by integer values and stores the actual elements in one array, the number of elements ever
    /// enqueued (number of calls to Enqueue) needs to be set beforehand. These arrays are statically stored in an object pool
    /// and are never completely cleared for performance reasons, so references to enqueued instances of T may exist for an
    /// arbitrary time (until the next queue uses that array and at least the same amount of elements are enqueued).
    /// </remarks>
    /// <typeparam name="T">Type of the stored objects, needs to implement <see cref="IWithPriority"/>.</typeparam>
    public class LinkedListPriorityQueue<T> : IDisposable
        where T: IWithPriority
    {
        // The large arrays (_elements and _links) are reused through an object pool because allocation of arrays of this size
        // (with easily a few 100k elements) is expensive and was bottlenecking the priority queue class.
        // (yes, Enqueue and Dequeue are that fast)

        /// <summary>
        /// Struct that saves the two large arrays used by Queue instances so they don't need to be reallocated every time.
        /// </summary>
        /// <remarks>
        /// Do not read any entry that was not overwriten by the same instance as the arrays are not cleaned of data from previous
        /// queues.
        /// </remarks>
        private struct DataArrays
        {
            public readonly T[] Elements;

            public readonly int[] Links;

            public DataArrays(T[] elements, int[] links)
            {
                Elements = elements;
                Links = links;
            }
        }

        /// <summary>
        /// Object pool of DataArrays which stores all arrays that are no longer in use.
        /// </summary>
        private static readonly ConcurrentBag<DataArrays> DataStack = new ConcurrentBag<DataArrays>();

        /// <summary>
        /// The smallest priority for which elements are currently stored.
        /// </summary>
        private uint _top = uint.MaxValue;
        
        /// <summary>
        /// The number of elements currently stored.
        /// </summary>
        private int _count;

        /// <summary>
        /// Returns true iff the queue currently holds no elements.
        /// </summary>
        public bool IsEmpty
        {
            get { return _count == 0; }
        }

        /// <summary>
        /// Stores the first element of the linked list of each priority. An entry p
        /// is only valid if <code>_lastElements[p] != 0</code>.
        /// 
        /// Entries are indexes of _elements and _links.
        /// </summary>
        /// <remarks>
        /// Initially filled with 0s but entries are never reset to 0.
        /// </remarks>
        private readonly int[] _firstElements;

        /// <summary>
        /// Stores the last element of the linked list of each priority. An entry is 0
        /// iff the linked list for that priority is empty.
        /// 
        /// Entries are indexes of _elements and _links.
        /// </summary>
        private readonly int[] _lastElements;

        /// <summary>
        /// Contains all nodes that are and were stored in this queue.
        /// </summary>
        /// <remarks>
        /// <code>_elements[0]</code> is not used as the index 0 is used as a special value.
        /// 
        /// <code>_elements[x]</code> for each <code>x</code> with <code>x >= _nextId</code> should
        /// never be used as it may contain elements of older queue instances.
        /// </remarks>
        private readonly T[] _elements;

        /// <summary>
        /// Contains the actual links of each element. If the value is 0, the element is either the last
        /// element of the linked list of its priority, or it was already dequeued from this queue.
        /// </summary>
        /// <remarks>
        /// The same index restriction as of _elements apply.
        /// </remarks>
        private readonly int[] _links;

        /// <summary>
        /// The index the next element that is enqueued will get in _elements and _links. Also represents the number
        /// of calls to Enqueue on this instance (plus 1).
        /// </summary>
        private int _nextId = 1;

        /// <summary>
        /// Creates a queue with the given maximum priority of stored nodes and the given number of calls to Enqueue
        /// that will ever be made.
        /// </summary>
        /// <param name="maxPriority">The maximum priority of nodes that will be stored.
        /// If a node with a higher priority is to be stored, an exception will be thrown.
        /// The value has no impact on runtime, only on used memory.</param>
        /// <param name="size">The number of calls to Enqueue supported by this Queue. If Enqueue is called more often,
        /// exceptions might be thrown by Enqueue. Overestimating the value does not hurt much if the class is used more than
        /// once with the same type parameter because the allocated arrays are reused.</param>
        public LinkedListPriorityQueue(int maxPriority, int size)
        {
            _firstElements = new int[maxPriority + 1];
            _lastElements = new int[maxPriority + 1];
            DataArrays current;
            if (!DataStack.TryTake(out current))
            {
                // If the pool is currently empty: create new Arrays of the necessary size.
                _elements = new T[size + 1];
                _links = new int[size + 1];
            }
            else if (current.Links.Length < size + 1)
            {
                // If the taken arrays are too small:
                // Create new Arrays of the necessary size, but at least twice the size of the taken arrays.
                // The taken arrays are thrown away since they will likely be too small more often than not and they
                // will be replaced by the newly created ones.
                var length = Math.Max(size + 1, current.Links.Length*2);
                _elements = new T[length];
                _links = new int[length];
            }
            else
            {
                // If the taken arrays are big enough: use them.
                _links = current.Links;
                _elements = current.Elements;
            }
        }

        /// <summary>
        /// Enqueue the given node. Always O(1).
        /// 
        /// Older nodes with the same priority are stored in front.
        /// </summary>
        /// <param name="node">The node to be stored.</param>
        public void Enqueue(T node)
        {
            var priority = node.Priority;
            var id = _nextId++;
            // Overwrite potentially old entries.
            _elements[id] = node;
            _links[id] = 0;

            _count++;
            var old = _lastElements[priority];
            if (old == 0)
            {
                // If the list of this priority was empty, set this index as first element.
                _firstElements[priority] = id;
                // If the priority is lower than the currently lowest, set it as lowest.
                if (priority < _top)
                {
                    _top = priority;
                }
            }
            else
            {
                // Just append the element to the list if it was not empty.
                _links[old] = id;
            }
            // Elements are appended at the end.
            _lastElements[priority] = id;
        }

        /// <summary>
        /// Returns the node with the lowest priority and removes it from the queue.
        /// 
        /// O(|lowest priority now - lowest priority before|), so practically constant in our use cases.
        /// </summary>
        public T Dequeue()
        {
            _count--;
            var id = _firstElements[_top];
            var next = _links[id];
            if (next == 0)
            {
                // If it is the last element of its linked list, mark the list as empty.
                _lastElements[_top] = 0;
                if (_count == 0)
                {
                    // If the whole queue is now empty, there is no lowest priority with elements.
                    _top = uint.MaxValue;
                }
                else
                {
                    // If not, find the next lowest priority with elements.
                    while (_lastElements[++_top] == 0)
                    {
                    }
                }
            }
            else
            {
                // If it is not the last element of its linked list, traverse the list one element further.
                _firstElements[_top] = next;
            }
            // Return the actual element of this index.
            return _elements[id];
        }

        /// <summary>
        /// Frees the large arrays used by this instance so they can be used by future instances.
        /// </summary>
        public void Dispose()
        {
            DataStack.Add(new DataArrays(_elements, _links));
        }
    }
}