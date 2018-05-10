using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using PoESkillTree.Computation.Core.Events;

namespace PoESkillTree.Computation.Core.NodeCollections
{
    public class NodeCollection<TProperty> : INodeCollection<TProperty>, ICountsSubsribers
    {
        private readonly ICollection<ICalculationNode> _nodes = new HashSet<ICalculationNode>();

        private readonly Dictionary<ICalculationNode, TProperty> _nodeProperties =
            new Dictionary<ICalculationNode, TProperty>();

        public void Add(ICalculationNode node, TProperty property)
        {
            _nodes.Add(node);
            _nodeProperties[node] = property;
            OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, node));
        }

        public void Remove(ICalculationNode node)
        {
            if (_nodes.Remove(node))
            {
                _nodeProperties.Remove(node);
                OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Remove, node));
            }
        }

        public IEnumerator<ICalculationNode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _nodes.Count;

        public int SubscriberCount => CollectionChanged?.GetInvocationList().Length ?? 0;

        public event CollectionChangeEventHandler CollectionChanged;

        public IReadOnlyDictionary<ICalculationNode, TProperty> NodeProperties => _nodeProperties;

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}