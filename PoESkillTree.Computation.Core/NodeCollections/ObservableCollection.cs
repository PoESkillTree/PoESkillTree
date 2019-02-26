using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Non-readonly implementation of <see cref="IObservableCollection{T}"/> based on sets.
    /// </summary>
    public class ObservableCollection<T> : IObservableCollection<T>, ICountsSubsribers
    {
        private readonly HashSet<T> _collection = new HashSet<T>();

        public void Add(T element)
        {
            if (_collection.Add(element))
            {
                OnCollectionChanged(CollectionChangedEventArgs.AddedSingle(element));
            }
        }

        public void Remove(T element)
        {
            if (_collection.Remove(element))
            {
                OnCollectionChanged(CollectionChangedEventArgs.RemovedSingle(element));
            }
        }

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _collection.Count;

        public int SubscriberCount
            => (CollectionChanged?.GetInvocationList().Length ?? 0)
               + (UntypedCollectionChanged?.GetInvocationList().Length ?? 0);

        public event CollectionChangedEventHandler<T> CollectionChanged;
        public event EventHandler UntypedCollectionChanged;

        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<T> e)
        {
            CollectionChanged?.Invoke(this, e);
            UntypedCollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}