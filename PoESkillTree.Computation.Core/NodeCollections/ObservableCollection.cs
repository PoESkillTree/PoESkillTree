using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class ObservableCollection<T> : IObservableCollection<T>, ICountsSubsribers
    {
        private readonly ICollection<T> _collection = new HashSet<T>();

        public void Add(T element)
        {
            _collection.Add(element);
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, element));
        }

        public void Remove(T element)
        {
            if (_collection.Remove(element))
            {
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, element));
            }
        }

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _collection.Count;

        public int SubscriberCount => CollectionChanged?.GetInvocationList().Length ?? 0;

        public event CollectionChangeEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}