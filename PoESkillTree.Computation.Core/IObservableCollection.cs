using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// A collection that raises <see cref="CollectionChangeEventHandler"/> events when its elements are changed.
    /// </summary>
    /// <remarks>
    /// This interface is only there for users that only care about the event so they don't have to care about the
    /// type parameter. For users that care about the elements, use <see cref="IObservableCollection{T}"/>.
    /// </remarks>
    public interface IObservableCollection : IEnumerable
    {
        // Not using INotifyCollectionChanged as NotifyCollectionChangedEventArgs is really clunky to use and this
        // interface won't be directly used by WPF anyway.
        /// <summary>
        /// Event that is raised when this collection's elements changed.
        /// </summary>
        event CollectionChangeEventHandler CollectionChanged;
    }

    /// <summary>
    /// A collection that raises <see cref="CollectionChangeEventHandler"/> events when its elements are changed.
    /// </summary>
    public interface IObservableCollection<out T> : IReadOnlyCollection<T>, IObservableCollection
    {
    }
}