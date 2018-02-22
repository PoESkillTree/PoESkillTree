using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class CoreStatGraph : IStatGraph
    {
        private readonly IStatNodeFactory _nodeFactory;

        private readonly Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> _nodes =
            new Dictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>>();

        private readonly Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>> _formNodeCollections
            = new Dictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();

        public CoreStatGraph(IStatNodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }

        private ISuspendableEventViewProvider<IDisposableNode> GetDisposableNode(NodeType nodeType) => 
            (ISuspendableEventViewProvider<IDisposableNode>) _nodes
                .GetOrAdd(nodeType, _ => _nodeFactory.Create(nodeType));

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeType nodeType) => 
            GetDisposableNode(nodeType);

        public IReadOnlyDictionary<NodeType, ISuspendableEventViewProvider<ICalculationNode>> Nodes => _nodes;

        public void RemoveNode(NodeType nodeType)
        {
            if (!_nodes.ContainsKey(nodeType))
                return;
            var node = GetDisposableNode(nodeType);
            node.DefaultView.Dispose();
            node.SuspendableView.Dispose();
            _nodes.Remove(nodeType);
        }

        private ModifierNodeCollection GetModifierNodeCollection(Form form) =>
            (ModifierNodeCollection) _formNodeCollections
                .GetOrAdd(form, _ => _nodeFactory.Create(form));

        public ISuspendableEventViewProvider<INodeCollection<Modifier>> GetFormNodeCollection(Form form) => 
            GetModifierNodeCollection(form);

        public IReadOnlyDictionary<Form, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _formNodeCollections;

        public void RemoveFormNodeCollection(Form form) => 
            _formNodeCollections.Remove(form);

        public void AddModifier(ISuspendableEventViewProvider<IDisposableNode> node, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier.Form);
            collection.Add(node, modifier);
            ModifierCount++;
        }

        public void RemoveModifier(ISuspendableEventViewProvider<IDisposableNode> node, Modifier modifier)
        {
            var collection = GetModifierNodeCollection(modifier.Form);
            collection.Remove(node);
            ModifierCount--;
        }

        public int ModifierCount { get; private set; }
    }
}