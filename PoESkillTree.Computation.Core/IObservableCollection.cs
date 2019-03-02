using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// A collection that raises <see cref="EventHandler"/> events when its elements are changed.
    /// </summary>
    /// <remarks>
    /// This interface is only there for users that only care about the event so they don't have to care about the
    /// type parameter. For users that care about the elements, use <see cref="IObservableCollection{T}"/>.
    /// </remarks>
    public interface IObservableCollection : IEnumerable
    {
        /// <summary>
        /// Event that is raised when this collection's elements changed.
        /// </summary>
        event EventHandler UntypedCollectionChanged;
    }

    /// <summary>
    /// A collection that raises <see cref="CollectionChangedEventHandler{T}"/> events when its elements are changed.
    /// </summary>
    public interface IObservableCollection<T>
        : IReadOnlyCollection<T>, INotifyCollectionChanged<T>, IObservableCollection
    {
    }
}