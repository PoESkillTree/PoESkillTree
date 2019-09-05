using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Lightweight list that gets its underlying array from ArrayPool.Shared to avoid allocations
    /// </summary>
    public class PooledList<T> : IReadOnlyList<T>, IDisposable
    {
        private readonly T[] _items;

        public PooledList(int minimumLength)
        {
            _items = ArrayPool<T>.Shared.Rent(minimumLength);
        }

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
            => _items.Take(Count).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Add(T item)
        {
            _items[Count] = item;
            Count++;
        }

        public void Dispose()
            => ArrayPool<T>.Shared.Return(_items);
    }
}