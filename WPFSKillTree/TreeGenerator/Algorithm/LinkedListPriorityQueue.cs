﻿namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Priority queue nodes used for LinkedListPriorityQueue.
    /// </summary>
    /// <typeparam name="T">Type of the parent class.</typeparam>
    public abstract class LinkedListPriorityQueueNode<T>
    {
        /// <summary>
        /// The Priority to insert this node at.
        /// </summary>
        internal int Priority;

        /// <summary>
        /// The node coming after this node in the queue.
        /// </summary>
        internal T Next;
    }

    /// <summary>
    /// Priority Queue based on a linked list and a lookup table for priorities.
    /// </summary>
    /// <remarks>
    /// Has O(1) Enqueue and Dequeue. Enqueue only if the range of priorites is
    /// not much bigger than the number of nodes stored at any point (best if the difference
    /// between the smallest and highest priority is the same or smaller than the number of
    /// nodes concurrently stored).
    /// 
    /// If the priorites are not discrete (no integer values) or have a range much bigger than
    /// the number of nodes being in the queue at one point, use HeapPriorityQueue.
    /// 
    /// No actual traversing through the linked list happens.
    /// </remarks>
    /// <typeparam name="T">Type of the stored objects</typeparam>
    public class LinkedListPriorityQueue<T>
        where T : LinkedListPriorityQueueNode<T>
    {
        private T _first;

        private T _last;

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
            if (Count++ == 0)
            {
                _first = _last = node;
            }
            else if (priority < _first.Priority)
            {
                node.Next = _first;
                _first = node;
            }
            else if (priority >= _last.Priority)
            {
                _last.Next = node;
                _last = node;
            }
            else
            {
                var index = priority;
                while (_prioritiyLookup[index] == null)
                    index--;
                var previous = _prioritiyLookup[index];
                var next = previous.Next;
                node.Next = next;
                previous.Next = node;
            }
            _prioritiyLookup[priority] = node;
        }

        /// <summary>
        /// Returns the node with the lowest priority and removes
        /// it from the queue. O(1)
        /// </summary>
        public T Dequeue()
        {
            var node = _first;
            if (Count-- == 1)
            {
                _first = null;
                _last = null;
            }
            else
            {
                _first = node.Next;
            }
            node.Next = null;
            var prio = node.Priority;
            if (_prioritiyLookup[prio] == node)
            {
                _prioritiyLookup[prio] = null;
            }
            return node;
        }
    }
}