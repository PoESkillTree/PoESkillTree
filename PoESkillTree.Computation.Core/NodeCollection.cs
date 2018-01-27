using System;
using System.Collections;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Core
{
    public class NodeCollection<TProperty> : INodeCollection<TProperty>
    {
        private readonly ICollection<ICalculationNode> _nodes = new HashSet<ICalculationNode>();

        private readonly Dictionary<ICalculationNode, TProperty> _nodeProperties =
            new Dictionary<ICalculationNode, TProperty>();

        public void Add(ICalculationNode node, TProperty property)
        {
            _nodes.Add(node);
            _nodeProperties[node] = property;
            OnCollectionChanged(new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Add, node));
        }

        public void Remove(ICalculationNode node)
        {
            if (_nodes.Remove(node))
            {
                _nodeProperties.Remove(node);
                OnCollectionChanged(new NodeCollectionChangeEventArgs(NodeCollectionChangeAction.Remove, node));
            }
        }

        public IEnumerator<ICalculationNode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _nodes.Count;

        public event EventHandler<NodeCollectionChangeEventArgs> CollectionChanged;

        public IReadOnlyDictionary<ICalculationNode, TProperty> NodeProperties => _nodeProperties;

        private void OnCollectionChanged(NodeCollectionChangeEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}