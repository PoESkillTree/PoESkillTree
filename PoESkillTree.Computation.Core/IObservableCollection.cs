using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PoESkillTree.Computation.Core
{
    // This interface is only there for users that only care about the event so they don't have to care about the
    // type parameter.
    public interface IObservableCollection : IEnumerable
    {
        // Not using INotifyCollectionChanged as NotifyCollectionChangedEventArgs is really clunky to use and this
        // interface won't be directly used by WPF anyway.
        event CollectionChangeEventHandler CollectionChanged;
    }

    public interface IObservableCollection<out T> : IReadOnlyCollection<T>, IObservableCollection
    {
    }
}