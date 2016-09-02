using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace POESKillTree.Utils
{
    /// <summary>
    /// ISet implementation that notifies on collection and property changes.
    /// 
    /// Uses a HashSet as wrapped storage so Contains, Add and Remove take constant time.
    /// </summary>
    public class ObservableSet<T> : ISet<T>, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ISet<T> _set;
        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private bool _hasRangeIncompatibleHandlers;

        public int Count
        {
            get { return _set.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public ObservableSet()
        {
            _set = new HashSet<T>();
        }

        public ObservableSet(IEnumerable<T> items)
        {
            _set = new HashSet<T>(items);
        }

        private void CheckReentrancy()
        {
            if (_monitor.IsBusy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
            {
                throw new InvalidOperationException(
                    "There was an attempt to change this collection during a CollectionChanged event");
            }
        }

        #region IEnumerable methods

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_set).GetEnumerator();
        }

        #endregion

        #region ISet methods

        public void UnionWith(IEnumerable<T> other)
        {
            CheckReentrancy();
            var added = other.Where(item => _set.Add(item)).ToList();
            if (added.Any())
                OnCollectionChanged(NotifyCollectionChangedAction.Add, added);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (Count == 0)
                return;

            // Only use other directly if its Contain method takes constant time and
            // uses the default EqualityComparer.
            var otherAsHashSet = other as HashSet<T>;
            if (otherAsHashSet != null)
            {
                if (otherAsHashSet.Comparer.Equals(EqualityComparer<T>.Default))
                {
                    IntersectWith(otherAsHashSet);
                    return;
                }
                IntersectWith(new HashSet<T>(otherAsHashSet));
                return;
            }
            var otherAsObservable = other as ObservableSet<T>;
            if (other is ObservableSet<T>)
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
            if (toRemove.Any())
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, toRemove);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (Count == 0)
                return;

            CheckReentrancy();
            var removed = other.Where(item => _set.Remove(item)).ToList();
            if (removed.Any())
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, removed);
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

            if (added.Any() || removed.Any())
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, added, removed);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _set.SetEquals(other);
        }

        public bool Add(T item)
        {
            CheckReentrancy();
            var r = _set.Add(item);
            if (r)
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
            return r;
        }

        #endregion

        #region ICollection methods

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            if (Count == 0)
                return;
            CheckReentrancy();
            var items = _set.ToList();
            _set.Clear();
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, items);
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            CheckReentrancy();
            var r = _set.Remove(item);
            if (r)
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            return r;
        }

        #endregion

        #region Events

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList items)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            using (_monitor.Enter())
            {
                try
                {
                    var newCount = args.NewItems?.Count ?? 0;
                    var oldCount = args.OldItems?.Count ?? 0;
                    if (_hasRangeIncompatibleHandlers && (newCount > 1 || oldCount > 1))
                        CollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    else
                        CollectionChanged?.Invoke(this, args);
                }
                catch (NotSupportedException)
                {
                    // This is for WPF default handlers that can't handle range updates.
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    _hasRangeIncompatibleHandlers = true;
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}