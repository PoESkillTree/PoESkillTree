using System;
using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    // TODO Adjust for paths
    public class CoreStatGraph : IStatGraph
    {
        private readonly IStatNodeFactory _nodeFactory;

        private readonly Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> _nodes =
            new Dictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>>();

        private readonly Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            _formNodeCollections =
                new Dictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>();

        public CoreStatGraph(IStatNodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }

        private IDisposableNodeViewProvider GetDisposableNode(NodeSelector selector) =>
            (IDisposableNodeViewProvider) _nodes
                .GetOrAdd(selector, _nodeFactory.Create);

        public ISuspendableEventViewProvider<ICalculationNode> GetNode(NodeSelector selector) =>
            GetDisposableNode(selector);

        public IReadOnlyDictionary<NodeSelector, ISuspendableEventViewProvider<ICalculationNode>> Nodes => _nodes;

        public ISuspendableEventViewProvider<IObservableCollection<PathDefinition>> Paths =>
            throw new NotImplementedException();

        public void RemoveNode(NodeSelector selector)
        {
            if (!_nodes.ContainsKey(selector))
                return;
            var node = GetDisposableNode(selector);
            node.Dispose();
            _nodes.Remove(selector);
        }

        private ModifierNodeCollection GetModifierNodeCollection(FormNodeSelector selector) =>
            (ModifierNodeCollection) _formNodeCollections
                .GetOrAdd(selector, _nodeFactory.Create);

        public ISuspendableEventViewProvider<INodeCollection<Modifier>>
            GetFormNodeCollection(FormNodeSelector selector) =>
            GetModifierNodeCollection(selector);

        public IReadOnlyDictionary<FormNodeSelector, ISuspendableEventViewProvider<INodeCollection<Modifier>>>
            FormNodeCollections => _formNodeCollections;

        public void RemoveFormNodeCollection(FormNodeSelector selector) =>
            _formNodeCollections.Remove(selector);

        public void AddModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var selector = new FormNodeSelector(modifier.Form, PathDefinition.MainPath);
            var collection = GetModifierNodeCollection(selector);
            collection.Add(node, modifier);
            ModifierCount++;
        }

        public void RemoveModifier(ISuspendableEventViewProvider<ICalculationNode> node, Modifier modifier)
        {
            var selector = new FormNodeSelector(modifier.Form, PathDefinition.MainPath);
            var collection = GetModifierNodeCollection(selector);
            collection.Remove(node, modifier);
            ModifierCount--;
        }

        public int ModifierCount { get; private set; }
    }
}