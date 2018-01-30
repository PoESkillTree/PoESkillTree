using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class CoreStatGraph : IStatGraph
    {
        private readonly IStat _stat;
        private readonly INodeFactory _nodeFactory;
        private readonly INodeCollectionFactory _nodeCollectionFactory;

        private readonly Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> _nodes =
            new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>();

        private readonly Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>> _formNodeCollections
            = new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();

        public CoreStatGraph(IStat stat, INodeFactory nodeFactory, INodeCollectionFactory nodeCollectionFactory)
        {
            _stat = stat;
            _nodeFactory = nodeFactory;
            _nodeCollectionFactory = nodeCollectionFactory;
        }

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeType nodeType) => 
            _nodes.GetOrAdd(nodeType, _ => _nodeFactory.Create(_stat, nodeType));

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> Nodes => _nodes;

        public void RemoveNode(NodeType nodeType)
        {
            if (!_nodes.TryGetValue(nodeType, out var node))
                return;
            node.DefaultView.Dispose();
            node.SuspendableView.Dispose();
            _nodes.Remove(nodeType);
        }

        private ModifierNodeCollection GetModifierNodeCollection(Form form) =>
            (ModifierNodeCollection) _formNodeCollections
                .GetOrAdd(form, _ => _nodeCollectionFactory.Create());

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(Form form) => 
            GetModifierNodeCollection(form);

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _formNodeCollections;

        public void RemoveFormNodeCollection(Form form) => 
            _formNodeCollections.Remove(form);

        public void AddModifier(Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier.Form);
            var node = _nodeFactory.Create(modifier.Value);
            collection.Add(modifier, node);
            ModifierCount++;
        }

        public bool RemoveModifier(Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier.Form);
            var node = collection.Remove(modifier);
            if (node == null)
            {
                return false;
            }
            node.DefaultView.Dispose();
            node.SuspendableView.Dispose();
            ModifierCount--;
            return true;
        }

        public int ModifierCount { get; private set; }
    }
}