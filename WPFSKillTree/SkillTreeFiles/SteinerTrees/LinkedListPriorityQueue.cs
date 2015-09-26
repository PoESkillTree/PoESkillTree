namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    /// Priority queue nodes used for LinkedListPriorityQueue.
    /// </summary>
    /// <typeparam name="T">Type of the stored objects. Probably the type of the parent class.</typeparam>
    public class LinkedListPriorityQueueNode<T>
    {
        /// <summary>
        /// The Priority to insert this node at.
        /// </summary>
        public int Priority { get; internal set; }

#if DEBUG
        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// Represents the order the node was inserted in
        /// </summary>
        public long InsertionIndex { get; internal set; }
#endif

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// True, if the node is currently stored in the queue.
        /// </summary>
        public bool IsInQueue { get; internal set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// The node coming before this node in the queue.
        /// </summary>
        public T Previous { get; internal set; }

        /// <summary>
        /// <b>Used by the priority queue - do not edit this value.</b>
        /// The node coming after this node in the queue.
        /// </summary>
        public T Next { get; internal set; }
    }

    /// <summary>
    /// Priority Queue based on a doubly linked list and a lookup table for
    /// priorities.
    /// 
    /// Has O(1) Enqueue, Dequeue and Remove. Enqueue only if the range of priorites is
    /// not much bigger than the number of nodes stored at any point (best if the difference
    /// between the smallest and highest priority is the same or smaller than the number of
    /// nodes concurrently stored).
    /// 
    /// If the priorites are not discret (no integer values) or have a range much bigger than
    /// the number of nodes being in the queue at one point, use HeapPriorityQueue.
    /// 
    /// No actual traversing through the linked list over more than 3 nodes happens.
    /// </summary>
    /// <typeparam name="T">Type of the stored objects</typeparam>
    public class LinkedListPriorityQueue<T>
        where T : LinkedListPriorityQueueNode<T>
    {
        private T _first;

        private T _last;

#if DEBUG
        private long _numNodesEverEnqueued;
#endif

        /// <summary>
        /// Number of nodes currently in the Queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Contains the last node stored currently for each priority.
        /// Priorities without nodes stored are null.
        /// </summary>
        private readonly T[] _prioritiyLookup;

        /// <summary>
        /// Creates a queue with the given maximum priority of stored nodes.
        /// </summary>
        /// <param name="maxPriority">The maximum priority of nodes that will be stored.
        /// If a node with a higher priority is to be stored, an exception will be thrown.
        /// The value has no impact on runtime, only on used memory.</param>
        public LinkedListPriorityQueue(int maxPriority)
        {
            _prioritiyLookup = new T[maxPriority + 1];
        }

        /// <summary>
        /// Enqueue the given node. O(1) if there is a node with the same
        /// or slightly lower priority already stored. Else O(n) with n being the
        /// difference between the given and the next lower priority already stored.
        /// </summary>
        /// <param name="node">The node to be stored.</param>
        /// <param name="priority">The priority of the node. Older nodes with
        /// the same priority are stored in front.</param>
        public void Enqueue(T node, int priority)
        {
            node.Priority = priority;
            if (Count == 0)
            {
                _first = _last = node;
            }
            else if (priority < _first.Priority)
            {
                node.Next = _first;
                _first.Previous = node;
                _first = node;
            }
            else if (priority >= _last.Priority)
            {
                node.Previous = _last;
                _last.Next = node;
                _last = node;
            }
            else
            {
                var index = priority;
                while (_prioritiyLookup[index] == null)
                {
                    index--;
                }
                var previous = _prioritiyLookup[index];
                var next = previous.Next;
                node.Next = next;
                previous.Next = node;
                node.Previous = previous;
                next.Previous = node;
            }

            _prioritiyLookup[priority] = node;
            node.IsInQueue = true;
            Count++;
#if DEBUG
            node.InsertionIndex = _numNodesEverEnqueued++;
#endif
        }

        /// <summary>
        /// Returns the node with the lowest priority and removes
        /// it from the queue. O(1)
        /// </summary>
        public T Dequeue()
        {
            var node = _first;
            if (Count == 1)
            {
                _first = null;
                _last = null;
            }
            else
            {
                _first = node.Next;
                _first.Previous = null;
            }

            RemoveFromPrioLookup(node, node.Priority, node.Previous);
            Count--;
            node.IsInQueue = false;
            return node;
        }

        /// <summary>
        /// Removes the given node from the queue.
        /// O(1)
        /// </summary>
        public void Remove(T node)
        {
            if (!node.IsInQueue)
            {
                return;
            }
            var previous = node.Previous;
            var next = node.Next;
            if (Count == 1)
            {
                _first = null;
                _last = null;
            }
            else if (previous == null)
            {
                _first = next;
                _first.Previous = null;
            }
            else if (next == null)
            {
                _last = previous;
                _last.Next = null;
            }
            else
            {
                previous.Next = next;
                next.Previous = previous;
            }

            RemoveFromPrioLookup(node, node.Priority, previous);
            Count--;
            node.IsInQueue = false;
        }

        private void RemoveFromPrioLookup(T node, int prio, T previous)
        {
            if (_prioritiyLookup[prio] == node)
            {
                if (previous == null || previous.Priority != prio)
                {
                    _prioritiyLookup[prio] = null;
                }
                else
                {
                    _prioritiyLookup[prio] = previous;
                }
            }
        }
    }
}