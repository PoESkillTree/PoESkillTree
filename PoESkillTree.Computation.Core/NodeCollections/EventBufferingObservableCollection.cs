using System.Collections.Generic;
using System.ComponentModel;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Extends <see cref="ObservableCollection{T}"/> with <see cref="IBufferableEvent{T}"/>.
    /// </summary>
    public class EventBufferingObservableCollection<T>
        : ObservableCollection<T>, IBufferableEvent<CollectionChangeEventArgs>
    {
        private readonly IEventBuffer _eventBuffer;

        public EventBufferingObservableCollection(IEventBuffer eventBuffer)
            => _eventBuffer = eventBuffer;

        protected override void OnCollectionChanged(CollectionChangeEventArgs e)
            => _eventBuffer.Buffer(this, e);

        public void Invoke(IReadOnlyList<CollectionChangeEventArgs> args)
        {
            var newArgs = args.Count == 1
                ? args[0]
                : new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);
            base.OnCollectionChanged(newArgs);
        }
    }
}