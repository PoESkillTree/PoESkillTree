using System.Collections.Generic;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Extends <see cref="ObservableCollection{T}"/> with <see cref="IBufferableEvent{T}"/>.
    /// </summary>
    public class EventBufferingObservableCollection<T>
        : ObservableCollection<T>, IBufferableEvent<CollectionChangedEventArgs<T>>
    {
        private readonly IEventBuffer _eventBuffer;

        public EventBufferingObservableCollection(IEventBuffer eventBuffer)
            => _eventBuffer = eventBuffer;

        protected override void OnCollectionChanged(CollectionChangedEventArgs<T> e)
            => _eventBuffer.Buffer(this, e);

        public void Invoke(CollectionChangedEventArgs<T> args)
            => base.OnCollectionChanged(args);

        public void Invoke(List<CollectionChangedEventArgs<T>> args)
        {
            var added = new HashSet<T>();
            var removed = new HashSet<T>();
            foreach (var e in args)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!removed.Remove(item))
                        added.Add(item);
                }
                foreach (var item in e.RemovedItems)
                {
                    if (!added.Remove(item))
                        removed.Add(item);
                }
            }
            base.OnCollectionChanged(CollectionChangedEventArgs.Replaced(added, removed));
        }
    }
}