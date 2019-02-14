using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    /// <summary>
    /// Non-readonly implementation of <see cref="INodeCollection{TProperty}"/> using
    /// <see cref="EventBufferingObservableCollection{T}"/>.
    /// </summary>
    public class NodeCollection<TProperty>
        : EventBufferingObservableCollection<(ICalculationNode node, TProperty property)>, INodeCollection<TProperty>
    {
        /// <summary>
        /// Creates a non-buffering instance
        /// </summary>
        public NodeCollection() : this(new ImmediateEventBuffer())
        {
        }

        /// <summary>
        /// Creates a buffering instance
        /// </summary>
        public NodeCollection(IEventBuffer eventBuffer) : base(eventBuffer)
        {
        }

        public void Add(ICalculationNode node, TProperty property) => Add((node, property));

        public void Remove(ICalculationNode node, TProperty property) => Remove((node, property));
    }
}