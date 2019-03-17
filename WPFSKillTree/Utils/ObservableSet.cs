using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    /// <summary>
    /// ISet implementation that notifies on collection and property changes.
    /// 
    /// Uses a HashSet as wrapped storage so Contains, Add and Remove take constant time.
    /// </summary>
    public class ObservableSet<T> : ISet<T>, IReadOnlyCollection<T>, INotifyCollectionChanged<T>, INotifyPropertyChanged
    {
        private readonly ISet<T> _set;
        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        public int Count => _set.Count;

        bool ICollection<T>.IsReadOnly => false;

        public ObservableSet()
            => _set = new HashSet<T>();

        public ObservableSet(IEnumerable<T> items)
            => _set = new HashSet<T>(items);

        private void CheckReentrancy()
        {
            if (_monitor.IsBusy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException(
                    "There was an attempt to change this collection during a CollectionChanged event");
            }
        }

        public IEnumerator<T> GetEnumerator()
            => _set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)_set).GetEnumerator();

        public void UnionWith(IEnumerable<T> other)
        {
            CheckReentrancy();
            var added = other.Where(item => _set.Add(item)).ToList();
            OnCollectionChanged(CollectionChangedEventArgs.Added(added));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (Count == 0)
                return;

            // Only use other directly if its Contain method takes constant time and
            // uses the default EqualityComparer.
            if (other is HashSet<T> otherAsHashSet)
            {
                if (otherAsHashSet.Comparer.Equals(EqualityComparer<T>.Default))
                {
                    IntersectWith(otherAsHashSet);
                    return;
                }
                IntersectWith(new HashSet<T>(otherAsHashSet));
                return;
            }
            if (other is ObservableSet<T> otherAsObservable)
            {
                IntersectWith(otherAsObservable);
                return;
            }
            IntersectWith(new HashSet<T>(other));
        }

        private void IntersectWith(ICollection<T> other)
        {
            CheckReentrancy();
            var toRemove = _set.Where(item => !other.Contains(item)).ToList();
            toRemove.ForEach(item => _set.Remove(item));
            OnCollectionChanged(CollectionChangedEventArgs.Removed(toRemove));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (Count == 0)
                return;

            CheckReentrancy();
            var removed = other.Where(item => _set.Remove(item)).ToList();
            OnCollectionChanged(CollectionChangedEventArgs.Removed(removed));
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (Count == 0)
            {
                UnionWith(other);
                return;
            }

            CheckReentrancy();

            var added = new List<T>();
            var removed = new List<T>();
            foreach (var item in other)
            {
                if (_set.Remove(item))
                {
                    removed.Add(item);
                }
                else if (_set.Add(item))
                {
                    added.Add(item);
                }
            }

            OnCollectionChanged(CollectionChangedEventArgs.Replaced(added, removed));
        }

        public void ResetTo(IEnumerable<T> other)
            => ExceptAndUnionWith(_set.ToList(), other);

        public void ExceptAndUnionWith(IEnumerable<T> toRemove, IEnumerable<T> toAdd)
        {
            CheckReentrancy();
            var added = new HashSet<T>();
            var removed = new HashSet<T>();
            foreach (var t in toRemove)
            {
                if (_set.Remove(t))
                    removed.Add(t);
            }
            foreach (var t in toAdd)
            {
                if (_set.Add(t) && !removed.Remove(t))
                    added.Add(t);
            }
            OnCollectionChanged(CollectionChangedEventArgs.Replaced(added, removed));
        }

        public bool IsSubsetOf(IEnumerable<T> other)
            => _set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other)
            => _set.IsSupersetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other)
            => _set.IsProperSupersetOf(other);

        public bool IsProperSubsetOf(IEnumerable<T> other)
            => _set.IsProperSubsetOf(other);

        public bool Overlaps(IEnumerable<T> other)
            => _set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other)
            => _set.SetEquals(other);

        public bool Add(T item)
        {
            CheckReentrancy();
            var r = _set.Add(item);
            if (r)
                OnCollectionChanged(CollectionChangedEventArgs.AddedSingle(item));
            return r;
        }

        void ICollection<T>.Add(T item)
            => Add(item);

        public void Clear()
        {
            if (Count == 0)
                return;
            CheckReentrancy();
            var items = _set.ToList();
            _set.Clear();
            OnCollectionChanged(CollectionChangedEventArgs.Removed(items));
        }

        public bool Contains(T item)
            => _set.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
            => _set.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            CheckReentrancy();
            var r = _set.Remove(item);
            if (r)
                OnCollectionChanged(CollectionChangedEventArgs.RemovedSingle(item));
            return r;
        }

        public void RemoveAndAdd(T toRemove, T toAdd)
        {
            CheckReentrancy();

            var (removed, added) = (_set.Remove(toRemove), _set.Add(toAdd));
            if (removed && added)
            {
                if (!EqualityComparer<T>.Default.Equals(toRemove, toAdd))
                    OnCollectionChanged(CollectionChangedEventArgs.ReplacedSingle(toAdd, toRemove));
            }
            else if (removed)
            {
                OnCollectionChanged(CollectionChangedEventArgs.RemovedSingle(toRemove));
            }
            else if (added)
            {
                OnCollectionChanged(CollectionChangedEventArgs.AddedSingle(toAdd));
            }
        }

        private void OnCollectionChanged(CollectionChangedEventArgs<T> args)
        {
            if (args.AddedItems.IsEmpty() && args.RemovedItems.IsEmpty())
                return;

            using (_monitor.Enter())
                CollectionChanged?.Invoke(this, args);

            if (args.AddedItems.Count != args.RemovedItems.Count)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public event CollectionChangedEventHandler<T> CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}